using UnityEngine;
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int batteryPartCount;
    [SerializeField] private int bandageCount;
    [SerializeField] private PowerType currentPower = PowerType.None;

    public int BatteryPartCount => batteryPartCount;
    public int BandageCount => bandageCount;
    public PowerType CurrentPower => currentPower;
    public bool HasPower => currentPower != PowerType.None;

    public static event System.Action<PlayerTeam, int> OnBatteryPartChanged;
    public static event System.Action<PlayerTeam, int> OnBandageChanged;
    public static event System.Action<PlayerTeam, PowerType> OnPowerGranted;
    public static event System.Action<PlayerTeam, PowerType> OnPowerUsed;

    private PlayerController owner;

    private void Awake()
    {
        owner = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PowerRewardSystem.OnPowerGranted += HandlePowerGranted;
    }

    private void OnDisable()
    {
        PowerRewardSystem.OnPowerGranted -= HandlePowerGranted;
    }

    private void HandlePowerGranted(PlayerTeam team, PowerType power)
    {
        if (team != owner.team) return;
        currentPower = power;
        OnPowerGranted?.Invoke(owner.team, currentPower);
    }

    public PowerType ConsumePower()
    {
        if (currentPower == PowerType.None) return PowerType.None;
        PowerType used = currentPower;
        currentPower = PowerType.None;
        OnPowerUsed?.Invoke(owner.team, used);
        return used;
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