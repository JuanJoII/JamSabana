// Responsibility: Automatically receive pieces when the player approaches.
// Trigger OnBatteryCompleted to trigger the roulette wheel and B.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BatteryStation : MonoBehaviour
{
    [Header("Config")]
    public PlayerTeam team; 
    public int partsRequired = 3;
    
    private float lastDepositTime = -10f;
    private const float depositCooldown = 0.5f;
    
    public static event System.Action<PlayerTeam> OnBatteryCompleted;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (Time.time - lastDepositTime < depositCooldown) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        if (player.team != team) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        if (inventory.BatteryPartCount >= partsRequired)
        {
            lastDepositTime = Time.time;
            inventory.ConsumeBatteryParts(partsRequired);
            OnBatteryCompleted?.Invoke(team);
        }
    }
}