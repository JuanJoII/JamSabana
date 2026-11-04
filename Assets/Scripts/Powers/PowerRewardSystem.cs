// Responsibility: The roulette wheel triggers this event when it selects a power.
// PlayerInventory listens for OnPowerGranted.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerRewardSystem : MonoBehaviour
{
    [Header("Ruletas — una por jugador")]
    public RouletteUI cuteRoulette;
    public RouletteUI darkRoulette;

    public static event System.Action<PlayerTeam, PowerType> OnPowerGranted;

    // Teams whose roulette is running right now.
    private static readonly HashSet<PlayerTeam> spinningTeams = new HashSet<PlayerTeam>();

    public static bool IsSpinning(PlayerTeam team) => spinningTeams.Contains(team);

    // Keeps the set clean if "Enter Play Mode without domain reload" is enabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => spinningTeams.Clear();

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
        // Already spinning for this team: ignore (never run two roulettes at once).
        if (!spinningTeams.Add(team)) return;

        RouletteUI roulette = team == PlayerTeam.Cute ? cuteRoulette : darkRoulette;
        StartCoroutine(RunRoulette(team, roulette));
    }

    private IEnumerator RunRoulette(PlayerTeam team, RouletteUI roulette)
    {
        PlayerController player = GetPlayer(team);
        PowerType result = PowerType.None;

        // SetInputEnabled(false) also zeroes the velocity.
        if (player != null) player.SetInputEnabled(false);

        try
        {
            yield return StartCoroutine(roulette.Spin(GetAvailablePowers()));
            result = roulette.GetResult();
        }
        finally
        {
            // Always released, even if something fails mid-spin.
            spinningTeams.Remove(team);
            if (player != null) player.SetInputEnabled(true);
        }

        if (result != PowerType.None) GrantPower(team, result);
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
        OnPowerGranted?.Invoke(team, power);
    }
}