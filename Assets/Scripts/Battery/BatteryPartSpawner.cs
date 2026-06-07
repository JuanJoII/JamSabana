using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el spawn dinámico de partes de batería en tandas completas de 3 piezas simultáneas.
/// Genera una tanda (Partes 1, 2 y 3) de inmediato al iniciar el gameplay y repite el proceso
/// tras un cooldown de 8 segundos cada vez que el jugador recolecta todas las piezas activas en su bando.
/// </summary>
[DisallowMultipleComponent]
public class BatteryPartSpawner : MonoBehaviour
{
    [Header("Configuración de Bando")]
    [SerializeField] private PlayerTeam targetTeam = PlayerTeam.Cute;

    [Header("Áreas de Spawn Específicas")]
    [Tooltip("Colisionadores de suelo (piso) para este bando. Si se deja vacío, buscará en toda la escena colisionadores con el Layer 'ValidBomb'.")]
    [SerializeField] private Collider[] spawnColliders;

    [Header("Prefabs de Partes")]
    [Tooltip("Deben asignarse exactamente 3 prefabs en el orden correspondientes a las partes 1, 2 y 3.")]
    [SerializeField] private GameObject[] batteryPartPrefabs;

    [Header("Configuración de Tiempo")]
    [Tooltip("Tiempo de espera en segundos antes de spawnear una nueva tanda de 3 piezas.")]
    [SerializeField] private float spawnInterval = 8f;

    private List<GameObject> activeParts = new List<GameObject>();
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
                    Debug.LogWarning($"[BatteryPartSpawner - {targetTeam}] El colisionador '{col.name}' asignado no pertenece al layer 'ValidBomb' (Capa {targetLayer}).");
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
        Debug.Log($"[BatteryPartSpawner - {targetTeam}] Inicializado con {validPlanes.Length} planos de spawn.");
    }

    /// <summary>
    /// Bucle principal del juego que spawnea una tanda al comenzar y repite el ciclo tras recolectarse todas.
    /// </summary>
    private IEnumerator SpawnLoopRoutine()
    {
        // 1. Spawnear la primera tanda de 3 piezas inmediatamente al iniciar el gameplay
        SpawnTrio();

        while (true)
        {
            // 2. Esperar mientras haya partes activas de este bando en el mapa
            while (HasActiveParts())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // 3. Una vez recogidas las 3 piezas, esperar el intervalo
            Debug.Log($"[BatteryPartSpawner - {targetTeam}] Tanda de piezas recogida. Spawneando nueva tanda en {spawnInterval} segundos.");
            yield return new WaitForSeconds(spawnInterval);

            // 4. Generar la tanda de 3 piezas al mismo tiempo
            SpawnTrio();
        }
    }

    /// <summary>
    /// Retorna si quedan partes de batería activas de este equipo en el mapa.
    /// </summary>
    private bool HasActiveParts()
    {
        activeParts.RemoveAll(part => part == null);
        return activeParts.Count > 0;
    }

    /// <summary>
    /// Instancia una tanda de 3 partes de batería al mismo tiempo (una del elemento 0, otra del 1 y otra del 2).
    /// </summary>
    private void SpawnTrio()
    {
        if (batteryPartPrefabs == null || batteryPartPrefabs.Length != 3)
        {
            Debug.LogWarning($"[BatteryPartSpawner - {targetTeam}] Se requieren exactamente 3 prefabs de partes de batería en el arreglo.");
            return;
        }

        for (int prefabIndex = 0; prefabIndex < 3; prefabIndex++)
        {
            Vector3 spawnPosition;
            const int maxAttempts = 15;
            bool positionFound = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (TryGetRandomSpawnPosition(out spawnPosition))
                {
                    // Evitar spawnear amontonados en la misma tanda
                    bool tooClose = false;
                    foreach (GameObject activePart in activeParts)
                    {
                        if (activePart != null && Vector3.Distance(activePart.transform.position, spawnPosition) < 1.5f)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    GameObject newPart = Instantiate(batteryPartPrefabs[prefabIndex], spawnPosition, Quaternion.identity);

                    Collectible collectible = newPart.GetComponent<Collectible>();
                    if (collectible != null)
                    {
                        collectible.worldTeam = targetTeam;
                    }

                    activeParts.Add(newPart);
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning($"[BatteryPartSpawner - {targetTeam}] No se pudo encontrar un punto de spawn válido para la parte index {prefabIndex}.");
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
