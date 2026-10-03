using UnityEngine;
/// <summary>
/// Handles brewing of Alchemy Recipes.
/// </summary>
public class AlchemyStationManager : MonoBehaviour
{
	public static AlchemyStationManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void OnEnable() => EventBus.OnAlchemyBrewRequested += HandleAlchemyBrewRequest;

	private void OnDisable() => EventBus.OnAlchemyBrewRequested -= HandleAlchemyBrewRequest;

	private void HandleAlchemyBrewRequest(ItemDataSo itemToBrew)
	{
		// 1. Check if the item has a Alchemy Recipe component
		if (!itemToBrew.TryGetProperty<ItemPropertyAlchemyRecipe>(out ItemPropertyAlchemyRecipe recipe))
		{
			Debug.LogWarning($"AlchemyStationManager: No Alchemy Recipe found for {itemToBrew.ItemName}. Cannot brew.");
			return;
		}

		// 2. Check for required resources in inventory
		if (!HasResources(recipe))
		{
			Debug.Log($"ALchemyStationManager: Not enough resources to craft {itemToBrew.ItemName}.");
			return;
		}

		// 3. Consume resources and brew the item
		ConsumeResources(recipe);
		GiveBrewedItem(itemToBrew);
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

	private static void GiveBrewedItem(ItemDataSo itemToBrew)
	{
		// 1. Create a new ItemInstance with the crafted ItemDataSo
		ItemInstance brewedItem = new ItemInstance(itemToBrew, 1);

		// 2. Try to add it to the inventory
		bool wasAdded = ItemStoragePlayer.Instance.AddItems(brewedItem);

		// 3. If adding to inventory failed
		if (!wasAdded)
		{
			Debug.Log($"AlchemyStationManager: Inventory is full. Dropping brewed item {brewedItem.DataSo.ItemName}.");

			// 4. Find the player position
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector3 dropPosition = player != null ? player.transform.position : Vector3.zero;

			// 5. Spawn the item
			GameObject itemToSpawn = itemToBrew.ItemObject != null ? itemToBrew.ItemObject : ItemStoragePlayer.Instance.DefaultItemObjectPrefab;
			GameObject droppedItem = Instantiate(itemToSpawn, dropPosition, Quaternion.identity);

			// 6. Set the itemData
			if (droppedItem.TryGetComponent(out ItemObject itemObject))
				itemObject.SetItemObject(brewedItem, dropPosition);
		}
	}
}
