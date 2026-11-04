using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el spawn dinámico y continuo de vendas (Bandages) para ambos bandos (Cute y Dark) en la misma escena.
/// Cada intervalo de tiempo genera una nueva tanda de vendas por bando, sin importar cuántas queden en el mapa.
/// </summary>
[DisallowMultipleComponent]
public class BandageSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TeamSpawnConfig
    {
        public PlayerTeam team;
        public string spawnTag;
        public GameObject bandagePrefab;
        [Tooltip("Cantidad de vendas que se generarán en cada tanda simultáneamente para este bando.")]
        public int amountPerWave = 1;
        
        [Header("Visualización en Inspector (No rellenar)")]
        public Collider[] detectedSpawnColliders;
    }

    [Header("Configuración de Equipos")]
    [SerializeField] private TeamSpawnConfig cuteTeamConfig = new TeamSpawnConfig 
    { 
        team = PlayerTeam.Cute, 
        spawnTag = "SpawnZoneCute",
        amountPerWave = 1
    };
    
    [SerializeField] private TeamSpawnConfig darkTeamConfig = new TeamSpawnConfig 
    { 
        team = PlayerTeam.Dark, 
        spawnTag = "SpawnZone",
        amountPerWave = 1
    };

    [Header("Configuración de Layers")]
    [Tooltip("Layers donde SÍ deben aparecer los drops (por defecto: ValidBomb).")]
    [SerializeField] private LayerMask validSpawnLayer = 1 << 6;

    [Tooltip("Layers donde NO deben aparecer los drops (por defecto: Default para edificios, muros y obstáculos).")]
    [SerializeField] private LayerMask invalidSpawnLayer = 1 << 0;

    [Tooltip("Radio de verificación para evitar que el drop aparezca incrustado dentro o pegado a un obstáculo.")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;

    [Header("Configuración de Tiempo global")]
    [Tooltip("Tiempo fijo en segundos para spawnear nuevas tandas de vendas.")]
    [SerializeField] private float spawnInterval = 10f;

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
        Debug.Log($"[BandageSpawner] Encontrados {config.detectedSpawnColliders.Length} bloques de suelo válidos para el bando {config.team} usando el tag '{config.spawnTag}'.");
    }

    /// <summary>
    /// Bucle que spawnea las vendas fijadas cada X segundos de manera infinita sin esperar a que se recolecten.
    /// </summary>
    private IEnumerator SpawnLoopRoutine(TeamSpawnConfig config)
    {
        // Primer spawn inmediato al iniciar
        SpawnWave(config);

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWave(config);
        }
    }

    /// <summary>
    /// Instancia una tanda de vendas al mismo tiempo usando la configuración del bando enviado.
    /// </summary>
    private void SpawnWave(TeamSpawnConfig config)
    {
        if (config.bandagePrefab == null)
        {
            Debug.LogWarning($"[BandageSpawner - {config.team}] No se asignó el prefab de venda en su configuración.");
            return;
        }

        if (config.detectedSpawnColliders == null || config.detectedSpawnColliders.Length == 0)
        {
            Debug.LogWarning($"[BandageSpawner - {config.team}] No hay bloques de suelo detectados. Imposible spawnear.");
            return;
        }

        // Lista local para evitar que las vendas de ESTA tanda se encimen entre sí
        List<Vector3> currentBatchPositions = new List<Vector3>();

        for (int i = 0; i < config.amountPerWave; i++)
        {
            Vector3 spawnPosition;
            const int maxAttempts = 15;
            bool positionFound = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (TryGetRandomSpawnPosition(config.detectedSpawnColliders, out spawnPosition))
                {
                    // Evitar que las vendas de esta misma tanda caigan amontonadas
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

                    // Instanciar el prefab de venda correspondiente al bando actual
                    GameObject newBandage = Instantiate(config.bandagePrefab, spawnPosition, Quaternion.identity);

                    Collectible collectible = newBandage.GetComponent<Collectible>();
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
                Debug.LogWarning($"[BandageSpawner - {config.team}] No se pudo encontrar un punto de spawn válido para la venda index {i} tras 15 intentos.");
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
