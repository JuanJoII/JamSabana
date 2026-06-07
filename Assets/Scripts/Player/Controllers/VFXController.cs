using UnityEngine;

public class VFXController : MonoBehaviour
{
    [Header("Jugador")]
    public GameObject vfxTeleport;      
    public GameObject vfxCollect;       

    private void OnEnable()
    {
        PlayerInventory.OnBatteryPartChanged += HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     += HandleBandageChanged;
        
        RiftObject.OnPlayerFellIntoRift      += HandlePlayerFellIntoRift;
        RiftPower.OnRiftPhaseStarted         += HandleRiftPhaseStarted;
        ConversionPower.OnConversionPhaseStarted += HandleConversionPhaseStarted;
    }

    private void OnDisable()
    {
        PlayerInventory.OnBatteryPartChanged -= HandleBatteryPartChanged;
        PlayerInventory.OnBandageChanged     -= HandleBandageChanged;
        RiftObject.OnPlayerFellIntoRift      -= HandlePlayerFellIntoRift;
        RiftPower.OnRiftPhaseStarted         -= HandleRiftPhaseStarted;
        ConversionPower.OnConversionPhaseStarted -= HandleConversionPhaseStarted;
    }

    private void HandleBatteryPartChanged(PlayerTeam team, int count)
    {
        PlayerController player = GetPlayer(team);
        if (player == null) return;
        SpawnVFX(vfxCollect, player.transform.position);
    }

    private void HandleBandageChanged(PlayerTeam team, int count)
    {
        PlayerController player = GetPlayer(team);
        if (player == null) return;
        SpawnVFX(vfxCollect, player.transform.position);
    }

    private void HandlePlayerFellIntoRift(PlayerController player)
    {
        SpawnVFX(vfxTeleport, player.transform.position);
    }

    private void HandleRiftPhaseStarted(PlayerTeam team)
    {
        PlayerController player = GetPlayer(team);
        if (player == null) return;
        SpawnVFX(vfxTeleport, player.transform.position);
    }

    private void HandleConversionPhaseStarted(PlayerTeam team)
    {
        PlayerController player = GetPlayer(team);
        if (player == null) return;
        SpawnVFX(vfxTeleport, player.transform.position);
    }

    private void SpawnVFX(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        Instantiate(prefab, position, Quaternion.identity);
    }

    private PlayerController GetPlayer(PlayerTeam team)
    {
        foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            if (p.team == team) return p;
        return null;
    }
}
