using UnityEngine;

/// Agranda, hace flotar y rotar el visual de un ítem para que destaque.
public class ItemHighlight : MonoBehaviour
{
    [Header("Referencia (opcional)")]
    [Tooltip("Hijo con el modelo. Si está vacío se usa este mismo transform.")]
    [SerializeField] private Transform visual;

    [Header("Tamaño")]
    [SerializeField] private float scaleMultiplier = 1.4f;

    [Header("Flotación")]
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Rotación")]
    [SerializeField] private float spinSpeed = 60f;

    private Vector3 baseLocalPos;
    private float phase;

    private void Awake()
    {
        if (visual == null) visual = transform;
        baseLocalPos = visual.localPosition;
        visual.localScale *= scaleMultiplier;
        phase = Random.value * Mathf.PI * 2f; // que no floten todos sincronizados
    }

    private void Update()
    {
        visual.localPosition = baseLocalPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);
        visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }
}