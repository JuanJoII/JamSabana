// Responsibility: Teleport the player to the enemy world, let them place rifts
// (only on valid spots), control the timer and return them to their world.

using UnityEngine;
using System.Collections;

public class RiftPower : MonoBehaviour
{
    [Header("Config")]
    public float duration = 20f;
    public int maxRifts = 3;

    [Header("Placement")]
    [SerializeField] private PlacementRules placementRules;
    [SerializeField] private float groundProbeHeight = 1f;
    [SerializeField] private float groundProbeDistance = 4f;

    [Header("Prefab")]
    public GameObject riftPrefab;

    public static event System.Action<PlayerTeam, Vector3> OnRiftCreated;
    public static event System.Action<PlayerTeam, Vector3> OnRiftPlacementRejected;
    public static event System.Action<PlayerTeam> OnRiftPhaseStarted;
    public static event System.Action<PlayerTeam> OnRiftPhaseEnded;

    private PlayerController owner;
    private Vector3 originalPosition;
    private int riftsPlaced;
    private Coroutine riftRoutine;
    private bool isInEnemyWorld;

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
        owner.TeleportTo(spawn.position);

        OnRiftPhaseStarted?.Invoke(owner.team);
        yield return new WaitForSeconds(duration);

        riftRoutine = null;
        ReturnToOwnWorld();
    }

    private void HandleInteract(PlayerController player)
    {
        if (player != owner) return;
        if (!isInEnemyWorld) return;
        if (riftsPlaced >= maxRifts) return;

        PlaceRift();
    }

    private void PlaceRift()
    {
        // Same rule as the bomb: floor below AND not inside a "NoPlacement" zone.
        Vector3 probe = owner.transform.position + Vector3.up * groundProbeHeight;
        bool valid = placementRules == null
                     || placementRules.TryGetPlacement(probe, groundProbeDistance, out _);

        if (!valid)
        {
            OnRiftPlacementRejected?.Invoke(owner.team, owner.transform.position);
            return;
        }

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
            ReturnToOwnWorld();
    }

    private void ReturnToOwnWorld()
    {
        if (riftRoutine != null)
        {
            StopCoroutine(riftRoutine);
            riftRoutine = null;
        }

        isInEnemyWorld = false;

        owner.TeleportTo(originalPosition);

        OnRiftPhaseEnded?.Invoke(owner.team);
    }
}