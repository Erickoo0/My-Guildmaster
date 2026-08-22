using System;
[Serializable]
/// <summary>
/// An instance of an item.
/// Holds changable data about an item.
/// </summary>
public class ItemInstance
{
	public int stackSize; // Tracks current Amount in this instance

	//future properties will go here

	public ItemInstance(ItemDataSo itemDataSo, int amount = 1)
	{
		DataSo = itemDataSo;
		stackSize = amount;
	}
	public ItemDataSo DataSo { get; private set; }
}
