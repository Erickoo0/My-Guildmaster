using UnityEngine;
using UnityEngine.InputSystem;
public class CraftingManager : MonoBehaviour
{

	[Header("testing")]
	[SerializeField] private ItemDataSo testItemToCraft;
	public static CraftingManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start() => EventBus.OnCraftItemRequested += HandleCraftItemRequest;

	private void Update()
	{
		// FIX: Using the New Input System to check if the 'N' key was pressed this frame
		if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame && testItemToCraft != null)
		{
			Debug.Log($"CraftingManager: Attempting to craft {testItemToCraft.ItemName} via hotkey...");
			EventBus.RequestCraftItem(testItemToCraft);
		}
	}

	private void OnDestroy() => EventBus.OnCraftItemRequested -= HandleCraftItemRequest;

	private void HandleCraftItemRequest(ItemDataSo itemToCraft)
	{
		// 1. Ensure the item actually has a recipe
		if (!itemToCraft.TryGetProperty<ItemPropertyCraftingRecipe>(out ItemPropertyCraftingRecipe recipe))
		{
			Debug.LogWarning($"CraftingManager: No recipe found for {itemToCraft.ItemName}. Cannot craft.");
			return;
		}

		// 2. Check for required resources
		if (!HasResources(recipe))
		{
			Debug.Log($"CraftingManager: Not enough resources to craft {itemToCraft.ItemName}.");
			return;
		}

		// 3. Consume resources and craft the item
		ConsumeResources(recipe);
		GiveCraftedItem(itemToCraft);
		Debug.Log($"CraftingManager: Crafted {itemToCraft.ItemName}!");
	}

	public bool HasResources(ItemPropertyCraftingRecipe recipe)
	{
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
		{
			int totalFound = 0;

			// Loop through player inventory to search for matching ItemDataSo 
			foreach (ItemInstance item in InventoryManager.Instance.itemsList)
			{
				if (item != null && item.DataSo == requiredResource.ItemDataSo)
					totalFound += item.stackSize;
			}

			// If not enough resource found, return false
			if (totalFound < requiredResource.Amount)
				return false;
		}

		// If all resources are found, return true
		return true;
	}

	private void ConsumeResources(ItemPropertyCraftingRecipe recipe)
	{
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requiredResource in recipe.RequiredResourcesList)
		{
			int amountLeftToConsume = requiredResource.Amount;

			// 1. Loop through player inventory to search for matching ItemDataSo 
			for (int i = 0; i < InventoryManager.Instance.itemsList.Length; i++)
			{
				ItemInstance item = InventoryManager.Instance.itemsList[i];

				// 2. Skip non matching items
				if (item == null || item.DataSo != requiredResource.ItemDataSo) continue;

				// 3. If slot has more than we need, subtract the required amount
				if (item.stackSize > amountLeftToConsume)
				{
					item.stackSize -= amountLeftToConsume;
					amountLeftToConsume = 0;

					InventoryManager.Instance.itemsList[i] = item; // Update the inventory. Need a better way to notify of inventory update later
					InventoryManager.Instance.ForceRefreshSlot(i);
					break;
				}
				// 4. If slot has exactly what we need, or not enough, drain the slot completely
				else
				{
					amountLeftToConsume -= item.stackSize;
					InventoryManager.Instance.itemsList[i] = null; // Clear the slot
					InventoryManager.Instance.ForceRefreshSlot(i);
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
		bool wasAdded = InventoryManager.Instance.AddItems(craftedItem);

		// 3. If adding to inventory failed
		if (!wasAdded)
		{
			Debug.Log($"CraftingManager: Inventory is full. Dropping crafted item {craftedItem.DataSo.ItemName}.");

			// 4. Find the player position
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector3 dropPosition = player != null ? player.transform.position : Vector3.zero;

			// 5. Spawn the item
			GameObject itemToSpawn = itemToCraft.ItemObject != null ? itemToCraft.ItemObject : InventoryManager.Instance.defaultItemObjectPrefab;
			GameObject droppedItem = Instantiate(itemToSpawn, dropPosition, Quaternion.identity);

			// 6. Set the itemData
			if (droppedItem.TryGetComponent(out ItemObject itemObject))
				itemObject.SetItemObject(craftedItem, dropPosition);
		}
	}
}
