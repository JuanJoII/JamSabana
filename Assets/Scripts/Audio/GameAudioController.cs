// Responsibility: Listens for game events and requests the AudioManager to play sounds.
// Contains no audio logic, only the event-to-sound mapping.
using UnityEngine;

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

    private AudioSource bombBeepSource;

    private void OnEnable()
    {
        PlayerInventory.OnBatteryPartChanged += HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     += HandleBandageChanged;
        BatteryStation.OnBatteryCompleted    += HandleBatteryCompleted;
        RouletteUI.OnRouletteTick            += HandleRouletteTick;
        RouletteUI.OnRouletteResult          += HandleRouletteResult;
        BombShip.OnBombPlaced                += HandleBombPlaced;
        BombObject.OnBombExploded            += HandleBombExploded;
        BombObject.OnBombDefused             += HandleBombDefused;
        RiftPower.OnRiftCreated              += HandleRiftCreated;
        RiftObject.OnRiftRepaired            += HandleRiftRepaired;
        RiftPower.OnRiftPhaseStarted         += HandleRiftPhaseStarted;
        ConversionPower.OnShootFired         += HandleConversionShoot;
        NPCController.OnNPCConverted         += HandleNPCConverted;
        RiftObject.OnPlayerFellIntoRift += HandlePlayerFellIntoRift;
    }

    private void OnDisable()
    {
        PlayerInventory.OnBatteryPartChanged -= HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     -= HandleBandageChanged;
        BatteryStation.OnBatteryCompleted    -= HandleBatteryCompleted;
        RouletteUI.OnRouletteTick            -= HandleRouletteTick;
        RouletteUI.OnRouletteResult          -= HandleRouletteResult;
        BombShip.OnBombPlaced                -= HandleBombPlaced;
        BombObject.OnBombExploded            -= HandleBombExploded;
        BombObject.OnBombDefused             -= HandleBombDefused;
        RiftPower.OnRiftCreated              -= HandleRiftCreated;
        RiftObject.OnRiftRepaired            -= HandleRiftRepaired;
        RiftPower.OnRiftPhaseStarted         -= HandleRiftPhaseStarted;
        ConversionPower.OnShootFired         -= HandleConversionShoot;
        NPCController.OnNPCConverted         -= HandleNPCConverted;
        RiftObject.OnPlayerFellIntoRift -= HandlePlayerFellIntoRift;
    }
    private void HandlePlayerFellIntoRift(PlayerController player)
    {
        Debug.Log($"[Audio] PlayerFellIntoRift | team: {player.team}");
        AudioManager.Instance.PlaySFX(playerTeleported, player.transform.position);
    }
    private void HandleBatteryPartChanged(PlayerTeam team, int count)
    {
        Debug.Log($"[Audio] BatteryPartChanged | team: {team} | count: {count}");
        AudioManager.Instance.PlaySFX(batteryPartCollected, Vector3.zero);
    }

    private void HandleBandageChanged(PlayerTeam team, int count)
    {
        Debug.Log($"[Audio] BandageChanged | team: {team} | count: {count}");
        AudioManager.Instance.PlaySFX(bandageCollected, Vector3.zero);
    }

    private void HandleBatteryCompleted(PlayerTeam team)
    {
        Debug.Log($"[Audio] BatteryCompleted | team: {team}");
        AudioManager.Instance.PlaySFX(batteryCompleted, Vector3.zero);
    }

    private void HandleRouletteTick()
    {
        Debug.Log("[Audio] RouletteTick");
        AudioManager.Instance.PlaySFX(rouletteTick, Vector3.zero);
    }

    private void HandleRouletteResult()
    {
        Debug.Log("[Audio] RouletteResult");
        AudioManager.Instance.PlaySFX(rouletteResult, Vector3.zero);
    }

    private void HandleBombPlaced(PlayerTeam team, Vector3 position)
    {
        Debug.Log($"[Audio] BombPlaced | team: {team} | pos: {position}");
        AudioManager.Instance.PlaySFX(bombPlaced, position);
        bombBeepSource = AudioManager.Instance.PlaySFXLoop(bombBeepLoop, position);
    }

    private void HandleBombExploded(PlayerTeam team, Vector3 position, float none)
    {
        Debug.Log($"[Audio] BombExploded | team: {team} | pos: {position}");
        AudioManager.Instance.StopSFXLoop(bombBeepSource);
        AudioManager.Instance.PlaySFX(bombExplode, position);
    }

    private void HandleBombDefused(PlayerTeam team)
    {
        Debug.Log($"[Audio] BombDefused | team: {team}");
        AudioManager.Instance.StopSFXLoop(bombBeepSource);
        AudioManager.Instance.PlaySFX(bombDefused, Vector3.zero);
    }

    private void HandleRiftCreated(PlayerTeam team, Vector3 position)
    {
        Debug.Log($"[Audio] RiftCreated | team: {team} | pos: {position}");
        AudioManager.Instance.PlaySFX(riftCreated, position);
    }

    private void HandleRiftRepaired(PlayerTeam team)
    {
        Debug.Log($"[Audio] RiftRepaired | team: {team}");
        AudioManager.Instance.PlaySFX(riftRepaired, Vector3.zero);
    }

    private void HandleRiftPhaseStarted(PlayerTeam team)
    {
        Debug.Log($"[Audio] RiftPhaseStarted | team: {team}");
        AudioManager.Instance.PlaySFX(playerTeleported, Vector3.zero);
    }

    private void HandleConversionShoot(PlayerController player)
    {
        Debug.Log($"[Audio] ConversionShoot | team: {player.team}");
        AudioManager.Instance.PlaySFX(conversionShoot, player.transform.position);
    }

    private void HandleNPCConverted(PlayerTeam team)
    {
        Debug.Log($"[Audio] NPCConverted | team: {team}");
        AudioManager.Instance.PlaySFX(npcConverted, Vector3.zero);
    }
}