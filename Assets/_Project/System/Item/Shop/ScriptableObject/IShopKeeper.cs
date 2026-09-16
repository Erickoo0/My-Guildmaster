/// <summary>
/// Defines a entity / prop as a shop keeper for the ShopManager to interact with
/// </summary>
public interface IShopKeeper
{
	ItemDataSo[] ShopList { get; }
}
