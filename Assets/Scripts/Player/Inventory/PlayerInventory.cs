using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int batteryPartCount;
    [SerializeField] private int bandageCount;

    public int BatteryPartCount => batteryPartCount;
    public int BandageCount => bandageCount;
    
    public static event System.Action<PlayerTeam, int> OnBatteryPartChanged;
    public static event System.Action<PlayerTeam, int> OnBandageChanged;

    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
    }

    public void AddBatteryPart()
    {
        batteryPartCount++;
        OnBatteryPartChanged?.Invoke(owner.team, batteryPartCount);
    }

    public void AddBandage()
    {
        bandageCount++;
        OnBandageChanged?.Invoke(owner.team, bandageCount);
    }

    public bool ConsumeBatteryParts(int amount)
    {
        if (batteryPartCount < amount) return false;
        batteryPartCount -= amount;
        OnBatteryPartChanged?.Invoke(owner.team, batteryPartCount);
        return true;
    }

    public bool ConsumeBandage()
    {
        if (bandageCount <= 0) return false;
        bandageCount--;
        OnBandageChanged?.Invoke(owner.team, bandageCount);
        return true;
    }
}