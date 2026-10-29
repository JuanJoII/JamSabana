// Responsibility: Play the "result reveal" animation: the winning icon pops out, holds, and fades with an activation pulse.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RouletteResultReveal : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Separate Image (NOT a child of the roulette). Disabled by default.")]
    public Image revealIcon;
    [Tooltip("Where the icon appears big (e.g. center of the player's half).")]
    public RectTransform revealPoint;
    [Tooltip("CanvasGroup on the roulette root, used to fade the wheel. Optional.")]
    public CanvasGroup rouletteGroup;

    [Header("Timing")]
    public float popDuration = 0.35f;
    public float holdDuration = 0.5f;
    public float activateDuration = 0.25f;
    public float wheelFadeDuration = 0.2f;

    [Header("Scale")]
    public float startScale = 0.5f;
    public float popScale = 1.6f;
    [Tooltip("Scale multiplier reached at the end of the activation pulse.")]
    public float activateScale = 2.2f;

    [Header("Curves")]
    public AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.7f, 1.15f), new Keyframe(1f, 1f));

    [Header("Debug")]
    public bool debugLogs = true;

    private bool isPlaying;

    private void OnDisable()
    {
        // Coroutines die when the object is deactivated, so the lock must be released here.
        isPlaying = false;
    }

    public IEnumerator Play(Sprite sprite, RectTransform from)
    {
        if (revealIcon == null || revealPoint == null) yield break;

        if (isPlaying)
        {
            if (debugLogs) Debug.LogWarning($"[Reveal:{name}] Play() llamado mientras ya estaba corriendo. Se ignora.", this);
            yield break;
        }

        if (sprite == null)
        {
            Debug.LogWarning($"[Reveal:{name}] El sprite del poder es NULL. Revisa bombSprite/riftSprite/conversionSprite en RouletteUI.", this);
        }

        isPlaying = true;
        if (debugLogs) Debug.Log($"[Reveal:{name}] Inicio. Sprite: {(sprite != null ? sprite.name : "null")}", this);

        RectTransform rt = revealIcon.rectTransform;
        revealIcon.sprite = sprite;
        revealIcon.enabled = true;
        revealIcon.gameObject.SetActive(true);

        Color c = revealIcon.color;
        c.a = 1f;
        revealIcon.color = c;

        Vector3 startPos = from != null ? from.position : revealPoint.position;
        Vector3 midPos = revealPoint.position;

        if (rouletteGroup != null) StartCoroutine(FadeGroup(rouletteGroup, 1f, 0f, wheelFadeDuration));

        // 1) Pop
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float k = popCurve.Evaluate(Mathf.Clamp01(t));
            rt.position = Vector3.LerpUnclamped(startPos, midPos, Mathf.Clamp01(t));
            rt.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, popScale, k);
            yield return null;
        }

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
        if (debugLogs) Debug.Log($"[Reveal:{name}] Fin.", this);
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