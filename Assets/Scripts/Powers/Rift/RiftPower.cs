// Responsibility: Teleport the player from the enemy world,
// control the timer and return them to their world.

using UnityEngine;
using System.Collections;

public class RiftPower : MonoBehaviour
{
    [Header("Config")]
    public float duration = 20f;
    public int maxRifts = 3;

    public static event System.Action<PlayerTeam, Vector3> OnRiftCreated;
    public static event System.Action<PlayerTeam> OnRiftPhaseStarted;
    public static event System.Action<PlayerTeam> OnRiftPhaseEnded;

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
        owner.transform.position = spawn.position;

        OnRiftPhaseStarted?.Invoke(owner.team);
        yield return new WaitForSeconds(duration);
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
        riftsPlaced++;
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