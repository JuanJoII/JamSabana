// Responsibility: The roulette wheel triggers this event when it selects a power.
// PlayerInventory listens for OnPowerGranted.

using UnityEngine;

public class PowerRewardSystem : MonoBehaviour
{
    public static event System.Action<PlayerTeam, PowerType> OnPowerGranted;

    private void OnEnable()
    {
        BatteryStation.OnBatteryCompleted += HandleBatteryCompleted;
    }

    private void OnDisable()
    {
        BatteryStation.OnBatteryCompleted -= HandleBatteryCompleted;
    }

    private void HandleBatteryCompleted(PlayerTeam team)
    {
        // Aquí va la lógica visual de la ruleta.
        // Cuando termina llama a:
        // GrantPower(team, powerElegido);
    }

    public void GrantPower(PlayerTeam team, PowerType power)
    {
        OnPowerGranted?.Invoke(team, power);
    }
}
