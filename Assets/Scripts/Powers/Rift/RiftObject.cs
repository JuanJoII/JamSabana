// Responsibility: To represent a rift in the world.
// Detects if the enemy player falls into it and teleports them.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiftObject : MonoBehaviour
{
    [Header("Config")]
    public float repairDistance = 2f;
    [Tooltip("Segundos que el jugador permanece en el mundo enemigo tras caer.")]
    public float trapDuration = 5f;

    public PlayerTeam attackingTeam;

    [Header("Prefab")]
    public GameObject bandageEffectPrefab;

    public static event System.Action<PlayerTeam> OnRiftRepaired;
    public static event System.Action<PlayerController> OnPlayerFellIntoRift;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnEnable()
    {
        PlayerController.OnInteractPressed += HandleRepairAttempt;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractPressed -= HandleRepairAttempt;
    }

    private void OnTriggerEnter(Collider other) => TryTrap(other);

    private void TryTrap(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || player.team == attackingTeam) return;

        OnPlayerFellIntoRift?.Invoke(player);

        Transform spawn = WorldSpawnPoints.GetSpawnFor(attackingTeam, player.team);
        player.TeleportTo(spawn.position);

        // Run on the PLAYER, not on the rift: if the rift is repaired/destroyed
        // meanwhile, the return still happens and nobody is stuck.
        player.StartCoroutine(ReturnAfterDelay(player, trapDuration));
    }

    private static IEnumerator ReturnAfterDelay(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform home = WorldSpawnPoints.GetSpawnFor(player.team, player.team);
        player.TeleportTo(home.position);
    }

    private void HandleRepairAttempt(PlayerController player)
    {
        if (player.team == attackingTeam) return;
        if (Vector3.Distance(player.transform.position, transform.position) > repairDistance) return;

        PlayerInventory inventory = player.inventory;
        if (inventory == null) return;

        if (!inventory.ConsumeBandage()) return;

        if (bandageEffectPrefab != null)
            Instantiate(bandageEffectPrefab, transform.position, Quaternion.identity);

        OnRiftRepaired?.Invoke(attackingTeam);
        Destroy(gameObject);
    }
}