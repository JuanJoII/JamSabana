// Responsibility: Listens for game events and requests the AudioManager to play sounds.
// Contains no audio logic, only the event-to-sound mapping.

using UnityEngine;
using System.Collections.Generic;
public class GameAudioController : MonoBehaviour
{
    [Header("Recolección")]
    public SoundData batteryPartCollected;
    public SoundData bandageCollected;

    [Header("Batería")]
    public SoundData batteryCompleted;

    [Header("Ruleta")]
    public SoundData rouletteTick;
    public SoundData rouletteResult;

    [Header("Bomba")]
    public SoundData bombPlaced;
    public SoundData bombBeepLoop;
    public SoundData bombExplode;
    public SoundData bombDefused;

    [Header("Rift")]
    public SoundData riftCreated;
    public SoundData riftRepaired;
    public SoundData playerTeleported;

    [Header("Conversión")]
    public SoundData conversionShoot;
    public SoundData npcConverted;

    [Header("Match")]
    public SoundData matchStart;
    public SoundData matchEnd;

    // Referencia al loop activo de la bomba para poder detenerlo
    private List<AudioSource> activeBombBeeps = new List<AudioSource>();
    private AudioSource bombBeepSource;
    private void OnEnable()
    {
        // Recolección
        PlayerInventory.OnBatteryPartChanged += HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     += HandleBandageChanged;

        // Batería
        BatteryStation.OnBatteryCompleted += HandleBatteryCompleted;

        // Ruleta
        RouletteUI.OnRouletteTick   += HandleRouletteTick;
        RouletteUI.OnRouletteResult += HandleRouletteResult;

        // Bomba
        BombShip.OnBombPlaced += HandleBombPlaced;
        BombObject.OnBombExploded            += HandleBombExploded;
        BombObject.OnBombDefused             += HandleBombDefused;

        // Rift
        RiftPower.OnRiftCreated          += HandleRiftCreated;
        RiftObject.OnRiftRepaired        += HandleRiftRepaired;
        RiftPower.OnRiftPhaseStarted     += HandleRiftPhaseStarted;

        // Conversión
        ConversionPower.OnShootFired += HandleConversionShoot;
        NPCController.OnNPCConverted += HandleNPCConverted;

        // Match
        // GameManager.OnMatchStarted += HandleMatchStarted;
        // GameManager.OnMatchEnded   += HandleMatchEnded;
    }

    private void OnDisable()
    {
        PlayerInventory.OnBatteryPartChanged -= HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     -= HandleBandageChanged;
        BatteryStation.OnBatteryCompleted    -= HandleBatteryCompleted;
        RouletteUI.OnRouletteTick            -= HandleRouletteTick;
        RouletteUI.OnRouletteResult          -= HandleRouletteResult;
        BombShip.OnBombPlaced -= HandleBombPlaced;
        BombObject.OnBombExploded            -= HandleBombExploded;
        BombObject.OnBombDefused             -= HandleBombDefused;
        RiftPower.OnRiftCreated              -= HandleRiftCreated;
        RiftObject.OnRiftRepaired            -= HandleRiftRepaired;
        RiftPower.OnRiftPhaseStarted         -= HandleRiftPhaseStarted;
        ConversionPower.OnShootFired         -= HandleConversionShoot;
        NPCController.OnNPCConverted         -= HandleNPCConverted;
    }

    // ── Handlers ────────────────────────────────────────────────

    private void HandleBatteryPartChanged(PlayerTeam team, int count)
    {
        // Solo suena cuando aumenta, no cuando se consume
        AudioManager.Instance.PlaySFX(batteryPartCollected, Vector3.zero);
    }

    private void HandleBandageChanged(PlayerTeam team, int count)
    {
        AudioManager.Instance.PlaySFX(bandageCollected, Vector3.zero);
    }

    private void HandleBatteryCompleted(PlayerTeam team)
    {
        AudioManager.Instance.PlaySFX(batteryCompleted, Vector3.zero);
    }

    private void HandleRouletteTick()
    {
        AudioManager.Instance.PlaySFX(rouletteTick, Vector3.zero);
    }

    private void HandleRouletteResult()
    {
        AudioManager.Instance.PlaySFX(rouletteResult, Vector3.zero);
    }

    private void HandleBombPlaced(PlayerTeam team, Vector3 position)
    {
        AudioManager.Instance.PlaySFX(bombPlaced, position);
        AudioSource beep = AudioManager.Instance.PlaySFXLoop(bombBeepLoop, position);
        activeBombBeeps.Add(beep);
    }

    private void StopAllBeeps()
    {
        foreach (AudioSource s in activeBombBeeps)
            AudioManager.Instance.StopSFXLoop(s);
        activeBombBeeps.Clear();
    }

    private void HandleBombExploded(PlayerTeam team, Vector3 position)
    {
        AudioManager.Instance.StopSFXLoop(bombBeepSource);
        AudioManager.Instance.PlaySFX(bombExplode, position);
    }

    private void HandleBombDefused(PlayerTeam team)
    {
        AudioManager.Instance.StopSFXLoop(bombBeepSource);
        AudioManager.Instance.PlaySFX(bombDefused, Vector3.zero);
    }

    private void HandleRiftCreated(PlayerTeam team, Vector3 position)
    {
        AudioManager.Instance.PlaySFX(riftCreated, position);
    }

    private void HandleRiftRepaired(PlayerTeam team)
    {
        AudioManager.Instance.PlaySFX(riftRepaired, Vector3.zero);
    }

    private void HandleRiftPhaseStarted(PlayerTeam team)
    {
        AudioManager.Instance.PlaySFX(playerTeleported, Vector3.zero);
    }

    private void HandleConversionShoot(PlayerController player)
    {
        AudioManager.Instance.PlaySFX(conversionShoot, player.transform.position);
    }

    private void HandleNPCConverted(PlayerTeam team)
    {
        AudioManager.Instance.PlaySFX(npcConverted, Vector3.zero);
    }
}