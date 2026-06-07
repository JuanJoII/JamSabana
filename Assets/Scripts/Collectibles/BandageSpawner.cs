using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el spawn dinámico de vendas (Bandages) en tandas simultáneas.
/// Genera una cantidad determinada de vendas de inmediato al iniciar el gameplay y repite el proceso
/// tras un cooldown cada vez que el jugador recolecta todas las vendas activas de su bando.
/// </summary>
[DisallowMultipleComponent]
public class BandageSpawner : MonoBehaviour
{
    [Header("Configuración de Bando")]
    [SerializeField] private PlayerTeam targetTeam = PlayerTeam.Cute;

    [Header("Áreas de Spawn Específicas")]
    [Tooltip("Colisionadores de suelo (piso) para este bando. Si se deja vacío, buscará en toda la escena colisionadores con el Layer 'ValidBomb'.")]
    [SerializeField] private Collider[] spawnColliders;

    [Header("Prefab de Vendaje")]
    [SerializeField] private GameObject bandagePrefab;

    [Header("Configuración de Tanda y Tiempo")]
    [Tooltip("Cantidad de vendas que se generarán en cada tanda simultáneamente.")]
    [SerializeField] private int amountPerWave = 1;
    [Tooltip("Tiempo de espera en segundos antes de spawnear una nueva tanda.")]
    [SerializeField] private float spawnInterval = 17f;

    private List<GameObject> activeBandages = new List<GameObject>();
    private Collider[] validPlanes;

    private void Start()
    {
        InitializeSpawnPlanes();
        StartCoroutine(SpawnLoopRoutine());
    }

    /// <summary>
    /// Inicializa y filtra las superficies válidas de spawn según el bando y el Layer 'ValidBomb' (Capa 6).
    /// </summary>
    private void InitializeSpawnPlanes()
    {
        int targetLayer = LayerMask.NameToLayer("ValidBomb");
        if (targetLayer == -1)
        {
            targetLayer = 6; // Capa física 6 por defecto si no se encuentra por nombre
        }

        List<Collider> planesList = new List<Collider>();

        if (spawnColliders != null && spawnColliders.Length > 0)
        {
            foreach (Collider col in spawnColliders)
            {
                if (col != null && col.gameObject.layer == targetLayer)
                {
                    planesList.Add(col);
                }
                else if (col != null)
                {
                    Debug.LogWarning($"[BandageSpawner - {targetTeam}] El colisionador '{col.name}' asignado no pertenece al layer 'ValidBomb' (Capa {targetLayer}).");
                }
            }
        }
        else
        {
            Collider[] allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (Collider col in allColliders)
            {
                if (col.gameObject.layer == targetLayer)
                {
                    planesList.Add(col);
                }
            }
        }

        validPlanes = planesList.ToArray();
        Debug.Log($"[BandageSpawner - {targetTeam}] Inicializado con {validPlanes.Length} planos de spawn.");
    }

    /// <summary>
    /// Bucle principal del juego que spawnea una tanda al comenzar y repite el ciclo tras recolectarse todas.
    /// </summary>
    private IEnumerator SpawnLoopRoutine()
    {
        // 1. Spawnear la primera tanda de vendas inmediatamente al iniciar el gameplay
        SpawnWave();

        while (true)
        {
            // 2. Esperar mientras haya vendas activas de este bando en el mapa
            while (HasActiveBandages())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // 3. Una vez recogidas todas las vendas, esperar el intervalo
            Debug.Log($"[BandageSpawner - {targetTeam}] Vendas recogidas. Spawneando nueva tanda en {spawnInterval} segundos.");
            yield return new WaitForSeconds(spawnInterval);

            // 4. Generar la tanda de vendas al mismo tiempo
            SpawnWave();
        }
    }

    /// <summary>
    /// Retorna si quedan vendas activas de este bando en el mapa.
    /// </summary>
    private bool HasActiveBandages()
    {
        activeBandages.RemoveAll(bandage => bandage == null);
        return activeBandages.Count > 0;
    }

    /// <summary>
    /// Instancia una tanda de vendas al mismo tiempo en posiciones aleatorias válidas.
    /// </summary>
    private void SpawnWave()
    {
        if (bandagePrefab == null)
        {
            Debug.LogWarning($"[BandageSpawner - {targetTeam}] No se asignó el prefab de venda.");
            return;
        }

        for (int i = 0; i < amountPerWave; i++)
        {
            Vector3 spawnPosition;
            const int maxAttempts = 15;
            bool positionFound = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (TryGetRandomSpawnPosition(out spawnPosition))
                {
                    // Evitar spawnear amontonadas en la misma tanda
                    bool tooClose = false;
                    foreach (GameObject activeBandage in activeBandages)
                    {
                        if (activeBandage != null && Vector3.Distance(activeBandage.transform.position, spawnPosition) < 1.5f)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    GameObject newBandage = Instantiate(bandagePrefab, spawnPosition, Quaternion.identity);

                    Collectible collectible = newBandage.GetComponent<Collectible>();
                    if (collectible != null)
                    {
                        collectible.worldTeam = targetTeam;
                    }

                    activeBandages.Add(newBandage);
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning($"[BandageSpawner - {targetTeam}] No se pudo encontrar un punto de spawn válido para la venda index {i}.");
            }
        }
    }

    /// <summary>
    /// Calcula una posición válida en el suelo dentro de los límites de un plano y fija la altura Y en 1.2f.
    /// </summary>
    private bool TryGetRandomSpawnPosition(out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;
        if (validPlanes == null || validPlanes.Length == 0) return false;

        Collider chosenCollider = validPlanes[Random.Range(0, validPlanes.Length)];
        Bounds bounds = chosenCollider.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float raycastOriginY = bounds.max.y + 5f;

        Vector3 rayOrigin = new Vector3(randomX, raycastOriginY, randomZ);

        int targetLayer = LayerMask.NameToLayer("ValidBomb");
        if (targetLayer == -1) targetLayer = 6;
        int layerMask = 1 << targetLayer;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 15f, layerMask))
        {
            spawnPos = new Vector3(hit.point.x, 1.2f, hit.point.z);
            return true;
        }

        return false;
    }
}
