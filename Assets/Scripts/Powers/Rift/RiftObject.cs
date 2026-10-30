// Responsibility: To represent a rift in the world.
// Detects if the enemy player falls into it and teleports them.
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiftObject : MonoBehaviour
{
    [Header("Config")]
    public float repairDistance = 2f;
    public PlayerTeam attackingTeam;

    [Header("Tiempos de Destierro e Inmunidad")]
    [Tooltip("Tiempo en segundos que el jugador desterrado pasa en el mundo rival")]
    public float banishDuration = 5f;
    [Tooltip("Tiempo de inmunidad contra grietas que recibe el jugador al retornar a su mundo")]
    public float returnImmunityDuration = 3f;
    
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

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (player.team == attackingTeam) return;

        // Si el jugador cuenta con inmunidad temporal (ej. recién regresó de un destierro), no lo teletransporta
        if (player.IsRiftImmune) return;

        OnPlayerFellIntoRift?.Invoke(player);
        TeleportToEnemyWorld(player);
    }

    private void TeleportToEnemyWorld(PlayerController player)
    {
        Transform spawn = WorldSpawnPoints.GetSpawnFor(attackingTeam, player.team);
        if (spawn != null)
        {
            player.transform.position = spawn.position;
        }

        // Ejecutar la corrutina en el PlayerController para que no se interrumpa si esta grieta es reparada o destruida
        player.StartCoroutine(ReturnAfterDelay(player, banishDuration));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        player.SetInputEnabled(false);
        yield return null; 

        PlayerTeam homeWorld = player.team;
        Transform spawn = WorldSpawnPoints.GetSpawnFor(homeWorld, player.team);
        if (spawn != null)
        {
            player.transform.position = spawn.position;
        }

        player.SetInputEnabled(true);
        // Otorga gracia/inmunidad para que el jugador pueda moverse o reparar la grieta sin quedar en bucle
        player.SetRiftImmunity(returnImmunityDuration);
    }

    private void HandleRepairAttempt(PlayerController player)
    {
        if (player.team == attackingTeam) return;
        if (Vector3.Distance(player.transform.position, transform.position) > repairDistance) return;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        if (!inventory.ConsumeBandage())
        {
            Debug.Log($"{player.team} intentó reparar grieta pero no tiene bandage");
            return;
        }
        
        if (bandageEffectPrefab != null)
            Instantiate(bandageEffectPrefab, transform.position, Quaternion.identity);

        OnRiftRepaired?.Invoke(attackingTeam);
        Destroy(gameObject);
    }
}
