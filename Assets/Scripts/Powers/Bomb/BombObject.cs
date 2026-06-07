using UnityEngine;
using System.Collections;

/// <summary>
/// Representa el comportamiento lógico y físico de la bomba una vez colocada en el mapa.
/// Maneja la cuenta regresiva, la desactivación y la detonación notificando a los sistemas visuales.
/// </summary>
public class BombObject : MonoBehaviour
{
    [Header("Configuración de Bomba")]
    public float countdownSeconds = 10f;
    public float explosionRadius = 4f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject cuteExplosionVFXPrefab;
    [SerializeField] private GameObject darkExplosionVFXPrefab;

    public static event System.Action<PlayerTeam, Vector3, float> OnBombExploded;
    public static event System.Action<PlayerTeam> OnBombDefused;

    public static void TriggerBombExploded(PlayerTeam team, Vector3 position, float radius) => OnBombExploded?.Invoke(team, position, radius);
    public static void TriggerBombDefused(PlayerTeam team) => OnBombDefused?.Invoke(team);

    private PlayerTeam attackingTeam;
    private Coroutine countdown;

    private void OnEnable()
    {
        PlayerController.OnInteractPressed += HandleInteract;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractPressed -= HandleInteract;
    }

    private void HandleInteract(PlayerController player)
    {
        if (player.team == attackingTeam) return;
        if (Vector3.Distance(player.transform.position, transform.position) > 2f) return;

        Defuse();
    }

    public void Initialize(PlayerTeam attacker, Vector3 position)
    {
        attackingTeam = attacker;
        transform.position = position;
        countdown = StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(countdownSeconds);
        Explode();
    }

    public void Defuse()
    {
        if (countdown != null) StopCoroutine(countdown);
        OnBombDefused?.Invoke(attackingTeam);
        Destroy(gameObject);
    }

    private void Explode()
    {
        GameObject chosenVFX = (attackingTeam == PlayerTeam.Cute) ? cuteExplosionVFXPrefab : darkExplosionVFXPrefab;

        // Instanciar VFX de explosión si está asignado
        if (chosenVFX != null)
        {
            Instantiate(chosenVFX, transform.position, Quaternion.identity);
        }
        else
        {
            // Fallback de depuración: crea una esfera visual básica si no hay prefab asignado
            GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.position = transform.position;
            debugSphere.transform.localScale = Vector3.one * (explosionRadius * 2f);

            Renderer sphereRenderer = debugSphere.GetComponent<Renderer>();
            if (sphereRenderer != null)
            {
                Color col = attackingTeam == PlayerTeam.Cute ? new Color(1f, 0.5f, 0.5f, 0.4f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
                sphereRenderer.material.color = col;
                Destroy(debugSphere.GetComponent<Collider>());
            }
            Destroy(debugSphere, 3f);
        }

        OnBombExploded?.Invoke(attackingTeam, transform.position, explosionRadius);
        Destroy(gameObject);
    }
}
