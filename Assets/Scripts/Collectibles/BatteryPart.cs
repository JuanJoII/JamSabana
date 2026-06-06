public class BatteryPart : Collectible
{
    protected override void Collect(PlayerInventory inventory)
    {
        inventory.AddBatteryPart();
    }
}
