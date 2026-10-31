using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el spawn dinámico y continuo de partes de batería para ambos bandos (Cute y Dark) en la misma escena.
/// Cada 5 segundos genera una nueva tanda de 3 piezas por bando, sin importar cuántas queden en el mapa.
/// </summary>
[DisallowMultipleComponent]
public class BatteryPartSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TeamSpawnConfig
    {
        public PlayerTeam team;
        public string spawnTag;
        public GameObject[] batteryPartPrefabs;
        
        [Header("Visualización en Inspector (No rellenar)")]
        public Collider[] detectedSpawnColliders;
    }

    [Header("Configuración de Equipos")]
    [SerializeField] private TeamSpawnConfig cuteTeamConfig = new TeamSpawnConfig 
    { 
        team = PlayerTeam.Cute, 
        spawnTag = "SpawnZoneCute" 
    };
    
    [SerializeField] private TeamSpawnConfig darkTeamConfig = new TeamSpawnConfig 
    { 
        team = PlayerTeam.Dark, 
        spawnTag = "SpawnZone" 
    };

    [Header("Configuración de Layers")]
    [Tooltip("Layers donde SÍ deben aparecer los drops (por defecto: ValidBomb).")]
    [SerializeField] private LayerMask validSpawnLayer = 1 << 6;

    [Tooltip("Layers donde NO deben aparecer los drops (por defecto: Default para edificios, muros y obstáculos).")]
    [SerializeField] private LayerMask invalidSpawnLayer = 1 << 0;

    [Tooltip("Radio de verificación para evitar que el drop aparezca incrustado dentro o pegado a un obstáculo.")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;

    [Header("Configuración de Tiempo global")]
    [Tooltip("Tiempo fijo en segundos para spawnear nuevas tandas.")]
    [SerializeField] private float spawnInterval = 5f;

    private void Awake()
    {
        EnsureDefaultLayers();
    }

    private void Start()
    {
        EnsureDefaultLayers();

        // Inicializa y busca los bloques de suelo válidos para ambos bandos automáticamente
        InitializeSpawnPlanes(cuteTeamConfig);
        InitializeSpawnPlanes(darkTeamConfig);

        // Iniciamos un bucle independiente para cada bando
        StartCoroutine(SpawnLoopRoutine(cuteTeamConfig));
        StartCoroutine(SpawnLoopRoutine(darkTeamConfig));
    }

    /// <summary>
    /// Garantiza que las máscaras tengan valores por defecto válidos si la escena aún no las había serializado.
    /// </summary>
    private void EnsureDefaultLayers()
    {
        if (validSpawnLayer.value == 0)
        {
            int validBombLayer = LayerMask.NameToLayer("ValidBomb");
            if (validBombLayer == -1) validBombLayer = 6;
            validSpawnLayer = 1 << validBombLayer;
        }

        if (invalidSpawnLayer.value == 0)
        {
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer == -1) defaultLayer = 0;
            invalidSpawnLayer = 1 << defaultLayer;
        }
    }

    /// <summary>
    /// Busca automáticamente todos los bloques que coincidan con el tag específico del bando
    /// y que pertenezcan al layer válido de spawn.
    /// Deja el resultado expuesto en el Inspector para poder visualizarlo y verificar que funcione.
    /// </summary>
    private void InitializeSpawnPlanes(TeamSpawnConfig config)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(config.spawnTag);
        List<Collider> planesList = new List<Collider>();

        foreach (GameObject obj in taggedObjects)
        {
            int objLayerMask = 1 << obj.layer;

            // Filtrar solo los bloques que estén en el layer permitido y no en el layer prohibido
            if ((validSpawnLayer.value & objLayerMask) == 0) continue;
            if ((invalidSpawnLayer.value & objLayerMask) != 0) continue;

            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                planesList.Add(col);
            }
        }

        config.detectedSpawnColliders = planesList.ToArray();
        Debug.Log($"[BatteryPartSpawner] Encontrados {config.detectedSpawnColliders.Length} bloques de suelo válidos para el bando {config.team} usando el tag '{config.spawnTag}'.");
    }

    /// <summary>
    /// Bucle que spawnea 3 piezas cada 5 segundos de manera infinita sin esperar a que se recolecten.
    /// </summary>
    private IEnumerator SpawnLoopRoutine(TeamSpawnConfig config)
    {
        // Primer spawn inmediato al iniciar
        SpawnTrio(config);

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnTrio(config);
        }
    }

    /// <summary>
    /// Instancia una tanda de 3 partes de batería al mismo tiempo usando la configuración del bando enviado.
    /// </summary>
    private void SpawnTrio(TeamSpawnConfig config)
    {
        if (config.batteryPartPrefabs == null || config.batteryPartPrefabs.Length != 3)
        {
            Debug.LogWarning($"[BatteryPartSpawner - {config.team}] Se requieren exactamente 3 prefabs de partes de batería en su configuración.");
            return;
        }

        if (config.detectedSpawnColliders == null || config.detectedSpawnColliders.Length == 0)
        {
            Debug.LogWarning($"[BatteryPartSpawner - {config.team}] No hay bloques de suelo detectados. Imposible spawnear.");
            return;
        }

        // Lista local para evitar que los 3 objetos de ESTA tanda se encimen entre sí
        List<Vector3> currentBatchPositions = new List<Vector3>();

        for (int prefabIndex = 0; prefabIndex < 3; prefabIndex++)
        {
            Vector3 spawnPosition;
            const int maxAttempts = 15;
            bool positionFound = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (TryGetRandomSpawnPosition(config.detectedSpawnColliders, out spawnPosition))
                {
                    // Evitar que las 3 piezas de esta misma tanda caigan en el mismo bloque exacto
                    bool tooClose = false;
                    foreach (Vector3 pos in currentBatchPositions)
                    {
                        if (Vector3.Distance(pos, spawnPosition) < 1.5f)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose) continue;

                    // Instanciar el prefab correspondiente al bando actual
                    GameObject newPart = Instantiate(config.batteryPartPrefabs[prefabIndex], spawnPosition, Quaternion.identity);

                    Collectible collectible = newPart.GetComponent<Collectible>();
                    if (collectible != null)
                    {
                        collectible.worldTeam = config.team;
                    }

                    currentBatchPositions.Add(spawnPosition);
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning($"[BatteryPartSpawner - {config.team}] No se pudo encontrar un punto de spawn válido para la parte index {prefabIndex} tras 15 intentos.");
            }
        }
    }

    /// <summary>
    /// Elige un bloque aleatorio de la lista detectada y calcula una posición sobre él mediante Raycast,
    /// verificando que caiga en validSpawnLayer y no sea obstruido por invalidSpawnLayer.
    /// </summary>
    private bool TryGetRandomSpawnPosition(Collider[] availableColliders, out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;

        // Elige uno de los pequeños bloques de suelo guardados al azar
        Collider chosenCollider = availableColliders[Random.Range(0, availableColliders.Length)];
        Bounds bounds = chosenCollider.bounds;

        // Calcula un punto arriba de ese bloque modular específico
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float raycastOriginY = bounds.max.y + 20f;

        Vector3 rayOrigin = new Vector3(randomX, raycastOriginY, randomZ);

        int combinedMask = validSpawnLayer.value | invalidSpawnLayer.value;

        // Lanza el rayo hacia abajo detectando tanto el suelo válido como posibles obstáculos encima
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 30f, combinedMask, QueryTriggerInteraction.Ignore))
        {
            int hitLayerMask = 1 << hit.collider.gameObject.layer;

            // Si el rayo impacta primero un objeto de la capa inválida (ej. edificio, árbol, muro), descartar
            if ((invalidSpawnLayer.value & hitLayerMask) != 0)
            {
                return false;
            }

            // Verificar que el impacto sea en la capa válida
            if ((validSpawnLayer.value & hitLayerMask) != 0)
            {
                Vector3 candidatePos = new Vector3(hit.point.x, 1.2f, hit.point.z);

                // Comprobar que en el punto donde aparecerá el drop no haya colisionadores de la capa inválida
                if (Physics.CheckSphere(candidatePos, obstacleCheckRadius, invalidSpawnLayer, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }

                spawnPos = candidatePos;
                return true;
            }
        }

        return false;
    }
}
