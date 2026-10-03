using UnityEngine;
/// <summary>
/// Handles crafting of Crafting Recipes.
/// </summary>
public class CraftingStationManager : MonoBehaviour
{
	public static CraftingStationManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void OnEnable() => EventBus.OnCraftItemRequested += HandleCraftItemRequest;

	private void OnDisable() => EventBus.OnCraftItemRequested -= HandleCraftItemRequest;

	private void HandleCraftItemRequest(ItemDataSo itemToCraft)
	{
		// 1. Check if the item has a Crafting Recipe component
		if (!itemToCraft.TryGetProperty<ItemPropertyCraftingRecipe>(out ItemPropertyCraftingRecipe recipe))
		{
			Debug.LogWarning($"CraftingStationManager: No Crafting Recipe found for {itemToCraft.ItemName}. Cannot craft.");
			return;
		}

		// 2. Check for required resources in inventory
		if (!HasResources(recipe))
		{
			Debug.Log($"CraftingStationManager: Not enough resources to craft {itemToCraft.ItemName}.");
			return;
		}

		// 3. Consume resources and craft the item
		ConsumeResources(recipe);
		GiveCraftedItem(itemToCraft);
		Debug.Log($"CraftingStationManager: Crafted {itemToCraft.ItemName}!");
	}

	public bool HasResources(ItemPropertyCraftingRecipe recipe)
	{
		// 1. Loop through all required resources in recipe
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
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

		// If all resources are found, return true
		return true;
	}

	private void ConsumeResources(ItemPropertyCraftingRecipe recipe)
	{
		// Loop through all required resources
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
		{
			int amountLeftToConsume = requiredResource.Amount;

			// 1. Loop through player inventory to search for matching ItemDataSo 
			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			{
				ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);

				// 2. Skip non matching items
				if (item == null || item.DataSo != requiredResource.ItemDataSo) continue;

				// 3. If slot has more than we need, subtract the required amount
				if (item.stackSize > amountLeftToConsume)
				{
					item.stackSize -= amountLeftToConsume;
					amountLeftToConsume = 0;

					ItemStoragePlayer.Instance.SetItem(i, item);
					break;
				}
				// 4. If slot has exactly what we need, or not enough, drain the slot completely
				else
				{
					amountLeftToConsume -= item.stackSize;
					ItemStoragePlayer.Instance.SetItem(i, null); // Clear the slot
				}

				// 5. If we have consumed enough, break out of the loop, otherwise continue to the next item
				if (amountLeftToConsume <= 0)
					break;
			}
		}
	}

	private static void GiveCraftedItem(ItemDataSo itemToCraft)
	{
		// 1. Create a new ItemInstance with the crafted ItemDataSo
		ItemInstance craftedItem = new ItemInstance(itemToCraft, 1);

		// 2. Try to add it to the inventory
		bool wasAdded = ItemStoragePlayer.Instance.AddItems(craftedItem);

		// 3. If adding to inventory failed
		if (!wasAdded)
		{
			Debug.Log($"CraftingStationManager: Inventory is full. Dropping crafted item {craftedItem.DataSo.ItemName}.");

			// 4. Find the player position
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector3 dropPosition = player != null ? player.transform.position : Vector3.zero;

			// 5. Spawn the item
			GameObject itemToSpawn = itemToCraft.ItemObject != null ? itemToCraft.ItemObject : ItemStoragePlayer.Instance.DefaultItemObjectPrefab;
			GameObject droppedItem = Instantiate(itemToSpawn, dropPosition, Quaternion.identity);

			// 6. Set the itemData
			if (droppedItem.TryGetComponent(out ItemObject itemObject))
				itemObject.SetItemObject(craftedItem, dropPosition);
		}
	}
}
