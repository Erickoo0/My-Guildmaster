using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manages the global player inventory.
/// Handles item storage, addition, and swapping, while notifying listeners of slot changes via events.
/// </summary>
public class ItemStoragePlayer : MonoBehaviour, ISaveable, IItemStorage
{

	public GameObject DefaultItemObjectPrefab;
	public ItemDatabase ItemDatabase;

	[Header("Inventory Settings")]
	[SerializeField] private int _inventorySize = 20;


	private ItemInstance[] _inventoryItemsList;
	public static ItemStoragePlayer Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this; // Assign This ID to variable

		if (ItemDatabase != null)
			ItemDatabase.Initialize();

		SetupInventory();
	}
	public int StorageCapacity => _inventorySize;

	public event Action<int> OnSlotUpdated;
	public bool CanDropToWorld => true;

	/// <summary>
	/// Swap items from specified indices 
	/// </summary>
	public void SwapItems(int indexA, int indexB)
	{
		// Check if indexes are out of range of _inventoryItemsList array
		if (indexA < 0 || indexA >= _inventoryItemsList.Length || indexB < 0 || indexB >= _inventoryItemsList.Length) return;

		// Swaps item from A and B using modern C# deconstruction
		(_inventoryItemsList[indexA], _inventoryItemsList[indexB]) = (_inventoryItemsList[indexB], _inventoryItemsList[indexA]);

		// Trigger event for BOTH slots involved in the swap
		OnSlotUpdated?.Invoke(indexA);
		OnSlotUpdated?.Invoke(indexB);
	}

	/// <summary>
	/// Drops an item from the specified inventory slot into the game world at the given position.
	/// </summary>
	public void DropItems(int index, Vector3 spawnPosition)
	{
		// Safety Check: Make sure the slot is not already empty
		if (_inventoryItemsList[index] == null) return;

		// 1. Get the item prefab
		GameObject itemPrefab = _inventoryItemsList[index].DataSo.ItemObject != null ? _inventoryItemsList[index].DataSo.ItemObject : DefaultItemObjectPrefab;

		// 2. Spawn the item
		GameObject droppedItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

		// 3. Pass the ItemData to the Item Object
		if (droppedItem.TryGetComponent(out ItemObject itemObject))
			itemObject.SetItemObject(_inventoryItemsList[index]);

		// Clear the slot and notify the UI
		_inventoryItemsList[index] = null;
		OnSlotUpdated?.Invoke(index);
	}

	/// <summary>
	/// Retrieves the ItemInstance at the specified inventory slot index.
	/// </summary>
	public ItemInstance GetItem(int index)
	{
		if (index < 0 || index >= _inventoryItemsList.Length) return null;
		return _inventoryItemsList[index];
	}

	/// <summary>
	/// Sets an ItemInstance in the specified inventory slot.
	/// </summary>
	public void SetItem(int index, ItemInstance item)
	{
		if (index < 0 || index >= _inventoryItemsList.Length) return;
		_inventoryItemsList[index] = item;
		OnSlotUpdated?.Invoke(index);
	}

	/// <summary>
	/// Refreshes the item at the specified inventory slot.
	/// </summary>
	/// <param name="index"></param>
	public void RefreshSlot(int index)
	{
		if (index < 0 || index >= _inventoryItemsList.Length) return;
		OnSlotUpdated?.Invoke(index);
	}

	//---- Load & Save Data Logic----
	public void PopulateSaveData(SaveData saveData)
	{
		// Create a list of SavedSlots
		List<SavedSlot> savedSlots = new List<SavedSlot>();

		// Loops through inventory and adds any non-empty slots into the savedSlots array
		for (int i = 0; i < _inventoryItemsList.Length; i++)
		{
			if (_inventoryItemsList[i] != null && _inventoryItemsList[i].DataSo != null)
			{
				// Create SavedSlots passing slot index #, itemID, and stackSize
				savedSlots.Add(new SavedSlot
				{
					index = i,
					itemID = _inventoryItemsList[i].DataSo.ItemID,
					itemStackSize = _inventoryItemsList[i].stackSize
				});
			}
		}

		// Send the SavedSLots list to SaveData
		saveData.SavedSlotList = savedSlots;
	}

	public void LoadFromSaveData(SaveData saveData)
	{
		// Clear current inventory
		SetupInventory();

		// Rebuild instances from the data inside the "box"
		foreach (SavedSlot savedSlot in saveData.SavedSlotList)
		{
			// Safety check: ensure index is within bounds
			if (savedSlot.index < 0 || savedSlot.index >= _inventoryItemsList.Length) continue;

			// Convert the itemIDs from savedSlot into ItemData
			ItemDataSo itemDataSo = ItemDatabase.GetItem(savedSlot.itemID);

			if (itemDataSo == null)
			{
				Debug.LogWarning($"[Inventory] Could not find item with ID: {savedSlot.itemID} in database.");
				continue;
			}

			// Use the ItemData to add ItemInstances to the _inventoryItemsList
			_inventoryItemsList[savedSlot.index] = new ItemInstance(itemDataSo, savedSlot.itemStackSize);
		}

		// 3. Notify the UI to refresh ALL slots
		for (int i = 0; i < _inventoryItemsList.Length; i++)
		{
			OnSlotUpdated?.Invoke(i);
		}
	}
	public event Action<ItemInstance> OnItemAddedToInventory;
	public event Action<int> OnActiveSlotIndexChanged;

	private void SetupInventory()
	{
		_inventoryItemsList = new ItemInstance[_inventorySize];

		// Initial active slot index 
		OnActiveSlotIndexChanged?.Invoke(0);
	}

	/// <summary>
	/// Adds an ItemInstance to the nearest empty inventory slot
	/// </summary>
	public bool AddItems(ItemInstance item)
	{
		// Try to stack first if the item is stackable
		if (item.DataSo.IsStackable == true)
		{
			for (int i = 0; i < _inventoryItemsList.Length; i++)
			{
				// Skip empty slots and slots with different items
				if (_inventoryItemsList[i] == null || _inventoryItemsList[i].DataSo != item.DataSo) continue;

				int spaceLeft = _inventoryItemsList[i].DataSo.MaxStackSize - _inventoryItemsList[i].stackSize;

				// Skip full slots
				if (spaceLeft <= 0) continue;

				// If whole new stack fits in current slot
				if (item.stackSize <= spaceLeft)
				{
					int amountAdded = item.stackSize; // Cache the Amount added for quest update

					_inventoryItemsList[i].stackSize += item.stackSize;
					OnSlotUpdated?.Invoke(i);
					OnItemAddedToInventory?.Invoke(item);

					EventBus.RequestUpdateQuestObjective(item.DataSo.ItemID, amountAdded);
					return true;
				}
				// If partial new stack fits in current slot
				else
				{
					_inventoryItemsList[i].stackSize = _inventoryItemsList[i].DataSo.MaxStackSize;
					item.stackSize -= spaceLeft;
					OnSlotUpdated?.Invoke(i);

					EventBus.RequestUpdateQuestObjective(item.DataSo.ItemID, spaceLeft);
					// Do not return true yet, code continues to find open slot for remainder
				}
			}
		}

		// If not stackable / No free stack available
		for (int i = 0; i < _inventoryItemsList.Length; i++)
		{
			if (_inventoryItemsList[i] == null)
			{
				_inventoryItemsList[i] = item; // Adds the item
				OnSlotUpdated?.Invoke(i);
				OnItemAddedToInventory?.Invoke(item);

				EventBus.RequestUpdateQuestObjective(item.DataSo.ItemID, item.stackSize);
				return true;
			}
		}

		Debug.unityLogger.Log("Iventory is full");
		return false; // If inventory is full
	}

	/// <summary>
	/// Removes the item from the specified slot index
	/// </summary>
	public void RemoveItems(int index)
	{
		if (index < 0 || index >= _inventoryItemsList.Length) return;
		ItemInstance item = _inventoryItemsList[index];
		item.stackSize--;
		if (item.stackSize <= 0) _inventoryItemsList[index] = null;

		OnSlotUpdated?.Invoke(index);
	}

	/// <summary>
	/// Invoke event signaling active slot index changed
	/// </summary>
	public void ChangeActiveSlot(int index) => OnActiveSlotIndexChanged?.Invoke(index);
}
