using System;
using UnityEngine;
/// <summary>
/// Handles brewing of Alchemy Recipes.
/// </summary>
public class AlchemyStationManager : MonoBehaviour, IItemStorage, IInteractable
{
	[Header("Interaction")]
	[SerializeField] private bool _interactable = true;

	[Header("Item Storage")]
	private ItemInstance[] _itemSlot = new ItemInstance[1];
	[Tooltip("The maximum number of items that can be stored in the station.")]
	public int StorageCapacity => 1;
	public bool CanDropToWorld => false;
	public event Action<int> OnSlotUpdated;

	#region Interaction Logic

	public bool CanInteract() => _interactable;

	public void Interact(ControllerPlayer player = null)
	{
		if (!CanInteract())
			return;

		EventBus.RequestOpenAlchemyStation(this);
	}

	public ItemInstance GetItem(int index) => _itemSlot[0];

	public void SetItem(int index, ItemInstance item)
	{
		_itemSlot[0] = item;
		RefreshSlot(0);
	}

	public void SwapItems(int indexA, int indexB) {} // Not used

	public void DropItems(int index, Vector3 spawnPosition)
	{
		// 1. Check if slot is empty
		if (_itemSlot[0] == null) return;

		// 2. Get the item prefab
		GameObject prefabToSpawn = _itemSlot[0].DataSo.ItemObject != null
			? _itemSlot[0].DataSo.ItemObject
			: ItemStoragePlayer.Instance.DefaultItemObjectPrefab;

		// 3. Instantiate the item
		GameObject droppedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
		if (droppedItem.TryGetComponent(out ItemObject itemObject))
			itemObject.SetItemObject(_itemSlot[0]);

		// 4. Remove the item from the slot
		_itemSlot[0] = null;
		RefreshSlot(0);
	}

	public void RefreshSlot(int index) => OnSlotUpdated?.Invoke(index);

	#endregion

	#region Brewing Logic

	public void TryBrew(ItemDataSo itemToBrew)
	{
		// 1. Check if the item has a Alchemy Recipe component
		if (!itemToBrew.TryGetProperty(out ItemPropertyAlchemyRecipe recipe))
			return;

		// 2. Check if output slot is already full
		if (_itemSlot[0] != null)
		{
			Debug.Log($"AlchemyStationManager: Output slot is already full.");
			return;
		}

		// 3. Check if the item has enough resources to brew
		if (!HasResources(recipe))
		{
			Debug.Log($"AlchemyStationManager: Not enough resources to brew {itemToBrew.ItemName}.");
			return;
		}

		// 4. Consume resources and brew the item
		ConsumeResources(recipe);
		SetItem(0, new ItemInstance(itemToBrew, 1));
		Debug.Log($"AlchemyStationManager: Successfully brewed {itemToBrew.ItemName}.");
	}

	public bool HasResources(ItemPropertyAlchemyRecipe recipe)
	{
		// 1. Loop through all required resources in recipe
		foreach (ItemPropertyAlchemyRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
		{
			int totalFound = 0;

			// 2. Loop through player inventory to search for matching ItemDataSo
			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			{
				// 3. Get the item from i slot
				ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);

				// 4. If item from i slot matches required resource, add to total
				if (item != null && item.DataSo == requiredResource.ItemDataSo)
					totalFound += item.stackSize;
			}

			// 5. Return false if total found is less than required amount
			if (totalFound < requiredResource.Amount)
				return false;
		}

		// 6. Return true if all required resources are found
		return true;
	}

	private void ConsumeResources(ItemPropertyAlchemyRecipe recipe)
	{
		// 1. Loop through all required resources
		foreach (ItemPropertyAlchemyRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
		{
			int amountLeftToConsume = requiredResource.Amount;

			// 2. Loop through player inventory to search for matching ItemDataSo
			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			{
				// 3. Get the item from i slot
				ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);

				// 4. If item from i slot matches is not the required resource, continue
				if (item == null || item.DataSo != requiredResource.ItemDataSo) continue;

				// 5. If slot has more than we need, subtract the required amount
				if (item.stackSize > amountLeftToConsume)
				{
					item.stackSize -= amountLeftToConsume;
					amountLeftToConsume = 0;

					ItemStoragePlayer.Instance.SetItem(i, item);
					break;
				}
				// 6. If slot has exactly what we need, or not enough, drain the slot completely
				else
				{
					amountLeftToConsume -= item.stackSize;
					ItemStoragePlayer.Instance.SetItem(i, null);
				}

				// 7. If we have consumed enough, break out of the loop, otherwise continue to the next item
				if (amountLeftToConsume <= 0)
					break;
			}
		}
	}

	#endregion
}
