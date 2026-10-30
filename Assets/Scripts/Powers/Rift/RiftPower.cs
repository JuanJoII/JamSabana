// Responsibility: Teleport the player from the enemy world,
// control the timer and return them to their world.

using UnityEngine;
using System.Collections;
using JamSabana.World;

public class RiftPower : MonoBehaviour
{
    [Header("Config")]
    public float duration = 20f;
    public int maxRifts = 3;

    [Header("Restricciones de Spawn")]
    [Tooltip("Distancia mínima a los puntos de spawn para permitir la colocación de una grieta")]
    public float minSpawnDistance = 4f;

    [Header("Prefab")]
    public GameObject riftPrefab;
    
    public static event System.Action<PlayerTeam, Vector3> OnRiftCreated;
    public static event System.Action<PlayerTeam> OnRiftPhaseStarted;
    public static event System.Action<PlayerTeam> OnRiftPhaseEnded;
    public static event System.Action<PlayerController> OnRiftPlacementBlocked;

    private PlayerController owner;
    private Vector3 originalPosition;
    private int riftsPlaced;
    private Coroutine riftRoutine;
    private bool isInEnemyWorld = false;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PlayerController.OnRiftActivated += HandleRiftActivated;
        PlayerController.OnInteractPressed += HandleInteract;
    }

    private void OnDisable()
    {
        PlayerController.OnRiftActivated -= HandleRiftActivated;
        PlayerController.OnInteractPressed -= HandleInteract;
    }

    private void HandleRiftActivated(PlayerController player)
    {
        if (player != owner) return;
        if (isInEnemyWorld) return;
        riftRoutine = StartCoroutine(RiftPhase());
    }

    private IEnumerator RiftPhase()
    {
        originalPosition = owner.transform.position;
        riftsPlaced = 0;
        isInEnemyWorld = true;
        
        PlayerTeam enemyWorld = owner.team == PlayerTeam.Cute ? PlayerTeam.Dark : PlayerTeam.Cute;
        Transform spawn = WorldSpawnPoints.GetSpawnFor(enemyWorld, owner.team);
        if (spawn != null)
        {
            owner.transform.position = spawn.position;
        }

        OnRiftPhaseStarted?.Invoke(owner.team);
        yield return new WaitForSeconds(duration);
        ReturnToOwnWorld();
    }

    private void HandleInteract(PlayerController player)
    {
        if (player != owner) return;
        if (!isInEnemyWorld) return;
        if (riftsPlaced >= maxRifts) return;

        if (!CanPlaceRiftAt(owner.transform.position))
        {
            Debug.LogWarning($"[RiftPower] No se puede colocar una grieta en esta posición: Zona segura de spawn.");
            OnRiftPlacementBlocked?.Invoke(owner);
            return;
        }

        PlaceRift();
    }

    /// <summary>
    /// Valida que la posición no esté dentro de un SpawnSafeZone ni a una distancia prohibida de los puntos de spawn.
    /// </summary>
    private bool CanPlaceRiftAt(Vector3 position)
    {
        // 1. Verificación por Trigger de Zona Segura (SpawnSafeZone)
        if (SpawnSafeZone.IsPlayerInSafeZone(owner) || SpawnSafeZone.IsPositionInAnySafeZone(position))
        {
            return false;
        }

        // 2. Verificación de respaldo por distancia a los Spawns del mundo actual
        PlayerTeam enemyWorld = owner.team == PlayerTeam.Cute ? PlayerTeam.Dark : PlayerTeam.Cute;

        Transform enemyHomeSpawn = WorldSpawnPoints.GetSpawnFor(enemyWorld, enemyWorld);
        if (enemyHomeSpawn != null && Vector3.Distance(position, enemyHomeSpawn.position) < minSpawnDistance)
        {
            return false;
        }

        Transform invaderSpawn = WorldSpawnPoints.GetSpawnFor(enemyWorld, owner.team);
        if (invaderSpawn != null && Vector3.Distance(position, invaderSpawn.position) < minSpawnDistance)
        {
            return false;
        }

        return true;
    }

    private void PlaceRift()
    {
        riftsPlaced++;
        
        if (riftPrefab != null)
        {
            GameObject rift = Instantiate(riftPrefab, owner.transform.position, Quaternion.identity);
            RiftObject riftObject = rift.GetComponent<RiftObject>();
            if (riftObject != null)
                riftObject.attackingTeam = owner.team;
        }

        OnRiftCreated?.Invoke(owner.team, owner.transform.position);

        if (riftsPlaced >= maxRifts)
        {
            if (riftRoutine != null) StopCoroutine(riftRoutine);
            ReturnToOwnWorld();
        }
    }

    private void ReturnToOwnWorld()
    {
        isInEnemyWorld = false;
        owner.transform.position = originalPosition;
        OnRiftPhaseEnded?.Invoke(owner.team); 
    }
}
