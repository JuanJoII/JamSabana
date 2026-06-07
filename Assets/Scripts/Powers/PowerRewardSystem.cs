// Responsibility: The roulette wheel triggers this event when it selects a power.
// PlayerInventory listens for OnPowerGranted.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PowerRewardSystem : MonoBehaviour
{
    [Header("Ruletas — una por jugador")]
    public RouletteUI cuteRoulette;
    public RouletteUI darkRoulette;

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
        RouletteUI roulette = team == PlayerTeam.Cute ? cuteRoulette : darkRoulette;
        StartCoroutine(RunRoulette(team, roulette));
    }
    private IEnumerator RunRoulette(PlayerTeam team, RouletteUI roulette)
    {
        PlayerController player = GetPlayer(team);
        if (player != null)
        {
            player.SetInputEnabled(false);
            player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }

        yield return StartCoroutine(roulette.Spin(GetAvailablePowers()));

        PowerType result = roulette.GetResult();
        Debug.Log($"Ruleta terminó | resultado: {result} | team: {team}");
        
        if (player != null) player.SetInputEnabled(true);

        GrantPower(team, result);
    }

    private List<PowerType> GetAvailablePowers()
    {
        List<PowerType> powers = new List<PowerType>
        {
            PowerType.Bomb,
            PowerType.Rift
        };
        
        if (NPCController.AllNPCs.Count > 0)
            powers.Add(PowerType.Conversion);

        return powers;
    }

    private PlayerController GetPlayer(PlayerTeam team)
    {
        foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            if (p.team == team) return p;
        return null;
        
    }
    public void GrantPower(PlayerTeam team, PowerType power)
    {
        Debug.Log($"GrantPower invocado | team: {team} | power: {power} | suscriptores: {OnPowerGranted?.GetInvocationList().Length}");
        OnPowerGranted?.Invoke(team, power);
    }
}