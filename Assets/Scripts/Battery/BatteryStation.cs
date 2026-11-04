// Responsibility: Automatically receive pieces when the player enters the station.
// Charges partsRequired ONCE per entry and fires OnBatteryCompleted to start the roulette.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BatteryStation : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam team;
    public int partsRequired = 3;

    public static event System.Action<PlayerTeam> OnBatteryCompleted;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // OnTriggerEnter (not Stay): the player is frozen inside the trigger while the roulette
    // spins, so Stay used to charge 3 more parts every 0.5s until the inventory was empty.
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || player.team != team) return;

        // Second safety net: never charge while this team's roulette is running.
        if (PowerRewardSystem.IsSpinning(team)) return;

        PlayerInventory inventory = player.inventory;
        if (inventory == null) return;

        // Returns false (and consumes nothing) if there are fewer than partsRequired.
        if (!inventory.ConsumeBatteryParts(partsRequired)) return;

        OnBatteryCompleted?.Invoke(team);
    }
}