/// <summary>Implemented by NPCs that open a shop through the craft UI (the bed-housed
/// merchant, the nomad leader). Lets ShopState read the shop without knowing the
/// concrete machine type.</summary>
public interface IShopkeeper
{
    CraftInfo Shop { get; }
}
