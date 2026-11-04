using UnityEngine;

/// Dibuja un anillo (y disco suave opcional) en el suelo bajo el objeto.
/// No requiere shader ni texturas: genera el sprite en runtime.
public class GroundMarker : MonoBehaviour
{
    [Header("Aspecto")]
    [SerializeField] private Color color = Color.yellow;
    [SerializeField] private float diameter = 1.2f;
    [SerializeField, Range(0.05f, 0.5f)] private float thickness = 0.15f;
    [SerializeField] private float heightOffset = 0.05f;
    [SerializeField] private int sortingOrder = 5;

    [Header("Relleno")]
    [SerializeField] private bool fillDisc = true;
    [SerializeField, Range(0f, 1f)] private float fillAlpha = 0.25f;

    [Header("Animación")]
    [SerializeField] private bool pulse = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.12f;

    private const int TexSize = 128;
    private Transform marker;
    private SpriteRenderer sr;
    private Texture2D tex;
    private Sprite sprite;

    private void Awake()
    {
        BuildSprite();

        var go = new GameObject("GroundMarker");
        go.layer = gameObject.layer;
        marker = go.transform;
        marker.SetParent(transform, false);

        sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    private void LateUpdate()
    {
        float s = diameter;
        if (pulse) s *= 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        // Compensa la escala del padre para que el tamaño sea el del Inspector
        Vector3 ls = transform.lossyScale;
        float parentScale = Mathf.Max(0.0001f, (ls.x + ls.y + ls.z) / 3f);

        marker.rotation = Quaternion.Euler(90f, 0f, 0f);
        marker.position = transform.position + Vector3.up * heightOffset;
        marker.localScale = Vector3.one * (s / parentScale);
        sr.color = color;
    }

    private void BuildSprite()
    {
        tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        float inner = 1f - thickness;
        const float aa = 0.04f;
        var px = new Color[TexSize * TexSize];

        for (int y = 0; y < TexSize; y++)
        for (int x = 0; x < TexSize; x++)
        {
            float dx = (x + 0.5f) / TexSize * 2f - 1f;
            float dy = (y + 0.5f) / TexSize * 2f - 1f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);

            float ring = Mathf.Clamp01((r - inner) / aa) * Mathf.Clamp01((1f - r) / aa);
            float disc = fillDisc ? fillAlpha * Mathf.Clamp01((inner - r) / aa + 1f) * (r < inner ? 1f : 0f) : 0f;

            px[y * TexSize + x] = new Color(1f, 1f, 1f, Mathf.Max(ring, disc));
        }

        tex.SetPixels(px);
        tex.Apply();
        sprite = Sprite.Create(tex, new Rect(0, 0, TexSize, TexSize), new Vector2(0.5f, 0.5f), TexSize);
    }

    private void OnDestroy()
    {
        if (sprite) Destroy(sprite);
        if (tex) Destroy(tex);
    }
}