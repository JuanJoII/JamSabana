public class Bandage : Collectible
{
    protected override void Collect(PlayerInventory inventory)
    {
        inventory.AddBandage();
    }
}