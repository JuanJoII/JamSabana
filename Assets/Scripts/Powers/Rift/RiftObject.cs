// Responsibility: To represent a rift in the world.
// Detects if the enemy player falls into it and teleports them.
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiftObject : MonoBehaviour
{
    [Header("Config")]
    public float repairDistance = 2f;
    
    public PlayerTeam attackingTeam;
    
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

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (player.team == attackingTeam) return;

        OnPlayerFellIntoRift?.Invoke(player);
        TeleportToEnemyWorld(player);
    }

    private void TeleportToEnemyWorld(PlayerController player)
    {
        Transform spawn = WorldSpawnPoints.GetSpawnFor(attackingTeam, player.team);
        player.transform.position = spawn.position;
        OnPlayerFellIntoRift?.Invoke(player);
        StartCoroutine(ReturnAfterDelay(player, 5f));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        player.SetInputEnabled(false);
        yield return null; 
        player.SetInputEnabled(true);

        PlayerTeam homeWorld = player.team;
        Transform spawn = WorldSpawnPoints.GetSpawnFor(homeWorld, player.team);
        player.transform.position = spawn.position;
    }

    private void HandleRepairAttempt(PlayerController player)
    {
        if (player.team == attackingTeam) return;
        if (Vector3.Distance(player.transform.position, transform.position) > repairDistance) return;
        if (!player.GetComponent<PlayerInventory>().ConsumeBandage()) return;

        OnRiftRepaired?.Invoke(attackingTeam);
        Destroy(gameObject);
    }
}