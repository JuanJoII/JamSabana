// Responsibility: Play the "result reveal": the winning icon bounces in at the center with a candy burst behind it, holds, and fades with an activation pulse.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteResultReveal : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Separate Image (NOT a child of the roulette). Disabled by default.")]
    public Image revealIcon;
    [Tooltip("Where the icon appears (center of the player's half). Must share the canvas with RevealIcon.")]
    public RectTransform revealPoint;
    [Tooltip("CanvasGroup on the roulette root, used to fade the wheel. Optional.")]
    public CanvasGroup rouletteGroup;

    [Header("Pop (bounce)")]
    public float popDuration = 0.4f;
    public float popScale = 1.6f;
    [Tooltip("Value 0..1 over time. Going above 1 creates the bounce overshoot.")]
    public AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.55f, 1.25f),
        new Keyframe(0.8f, 0.92f),
        new Keyframe(1f, 1f));

    [Header("Timing")]
    public float holdDuration = 0.5f;
    public float activateDuration = 0.25f;
    public float wheelFadeDuration = 0.2f;

    [Header("Activation Pulse")]
    public float activateScale = 2.2f;

    [Header("Candy Burst")]
    public bool enableBurst = true;
    [Tooltip("Sprite of each candy piece. If empty, pieces are plain squares.")]
    public Sprite burstSprite;
    public Color[] burstColors = new Color[]
    {
        new Color(1f, 0.45f, 0.75f),
        new Color(1f, 0.9f, 0.45f),
        new Color(0.55f, 0.9f, 1f),
        new Color(0.8f, 0.65f, 1f)
    };
    public int burstCount = 14;
    [Tooltip("How far pieces travel, in canvas units.")]
    public float burstDistance = 220f;
    [Tooltip("Min / max size of each piece.")]
    public Vector2 burstSize = new Vector2(24f, 48f);
    public float burstDuration = 0.55f;

    [Header("Puff Ring (optional)")]
    [Tooltip("Soft circle Image placed BEFORE RevealIcon in the Hierarchy so it draws behind. Disabled by default.")]
    public Image puffImage;
    public float puffStartScale = 0.4f;
    public float puffEndScale = 2.2f;
    public float puffDuration = 0.45f;
    [Range(0f, 1f)] public float puffAlpha = 0.8f;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool isPlaying;
    private readonly List<Image> pool = new List<Image>();

    private void OnDisable()
    {
        // Coroutines die when the object is deactivated, so everything must be cleaned here.
        isPlaying = false;
        if (revealIcon != null) revealIcon.gameObject.SetActive(false);
        if (puffImage != null) puffImage.gameObject.SetActive(false);
        for (int i = 0; i < pool.Count; i++)
            if (pool[i] != null) pool[i].gameObject.SetActive(false);
        if (rouletteGroup != null) rouletteGroup.alpha = 1f;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < pool.Count; i++)
            if (pool[i] != null) Destroy(pool[i].gameObject);
        pool.Clear();
    }

    // 'from' is kept so RouletteUI does not need changes, but it is no longer used.
    public IEnumerator Play(Sprite sprite, RectTransform from)
    {
        if (revealIcon == null || revealPoint == null) yield break;

        if (isPlaying)
        {
            if (debugLogs) Debug.LogWarning($"[Reveal:{name}] Play() ignored: already running.", this);
            yield break;
        }

        if (sprite == null)
            Debug.LogWarning($"[Reveal:{name}] Power sprite is NULL. Check bombSprite/riftSprite/conversionSprite in RouletteUI.", this);

        isPlaying = true;

        RectTransform rt = revealIcon.rectTransform;
        revealIcon.sprite = sprite;
        revealIcon.enabled = true;
        revealIcon.gameObject.SetActive(true);

        Color c = revealIcon.color;
        c.a = 1f;
        revealIcon.color = c;

        // Appears directly at the center: no travel.
        rt.position = revealPoint.position;
        rt.localScale = Vector3.zero;
        Vector3 centerLocal = rt.parent.InverseTransformPoint(revealPoint.position);

        if (rouletteGroup != null) StartCoroutine(FadeGroup(rouletteGroup, 1f, 0f, wheelFadeDuration));
        if (enableBurst) StartCoroutine(Burst(centerLocal));
        if (puffImage != null) StartCoroutine(Puff());

        // 1) Pop with bounce
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float k = popCurve.Evaluate(Mathf.Clamp01(t));
            rt.localScale = Vector3.one * (popScale * k);
            yield return null;
        }
        rt.localScale = Vector3.one * popScale;

        // 2) Hold
        yield return new WaitForSeconds(holdDuration);

        // 3) Activation pulse: scale up and fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / activateDuration;
            float k = Mathf.Clamp01(t);
            rt.localScale = Vector3.one * Mathf.Lerp(popScale, activateScale, k);
            c.a = 1f - k;
            revealIcon.color = c;
            yield return null;
        }

        revealIcon.gameObject.SetActive(false);
        if (rouletteGroup != null) rouletteGroup.alpha = 1f; // reset for the next spin

        isPlaying = false;
    }

    private void EnsurePool()
    {
        Transform parent = revealIcon.transform.parent;
        int sibling = revealIcon.transform.GetSiblingIndex();

        while (pool.Count < burstCount)
        {
            GameObject go = new GameObject("BurstPiece", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            go.SetActive(false);
            go.transform.SetSiblingIndex(sibling); // inserted before the icon = drawn behind it
            sibling++;
            pool.Add(img);
        }
    }

    private IEnumerator Burst(Vector3 centerLocal)
    {
        EnsurePool();

        int n = Mathf.Min(burstCount, pool.Count);
        Vector2[] dirs = new Vector2[n];
        float[] dists = new float[n];
        float[] spins = new float[n];
        float[] baseScale = new float[n];

        for (int i = 0; i < n; i++)
        {
            float a = ((i + Random.value * 0.6f) / n) * Mathf.PI * 2f;
            dirs[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            dists[i] = burstDistance * Random.Range(0.6f, 1f);
            spins[i] = Random.Range(-360f, 360f);
            baseScale[i] = 1f;

            Image img = pool[i];
            float size = Random.Range(burstSize.x, burstSize.y);
            img.sprite = burstSprite;
            img.color = burstColors != null && burstColors.Length > 0
                ? burstColors[Random.Range(0, burstColors.Length)]
                : Color.white;
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.rectTransform.localPosition = centerLocal;
            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            img.rectTransform.localScale = Vector3.one;
            img.gameObject.SetActive(true);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / burstDuration;
            float k = Mathf.Clamp01(t);
            float easeOut = 1f - (1f - k) * (1f - k);

            for (int i = 0; i < n; i++)
            {
                Image img = pool[i];
                img.rectTransform.localPosition = centerLocal + (Vector3)(dirs[i] * dists[i] * easeOut);
                img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, spins[i] * k);
                img.rectTransform.localScale = Vector3.one * Mathf.Lerp(baseScale[i], 0f, k * k);
            }
            yield return null;
        }

        for (int i = 0; i < n; i++) pool[i].gameObject.SetActive(false);
    }

    private IEnumerator Puff()
    {
        RectTransform prt = puffImage.rectTransform;
        prt.position = revealPoint.position;
        puffImage.gameObject.SetActive(true);

        Color pc = puffImage.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / puffDuration;
            float k = Mathf.Clamp01(t);
            float easeOut = 1f - (1f - k) * (1f - k);
            prt.localScale = Vector3.one * Mathf.Lerp(puffStartScale, puffEndScale, easeOut);
            pc.a = Mathf.Lerp(puffAlpha, 0f, k);
            puffImage.color = pc;
            yield return null;
        }

        puffImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }
}