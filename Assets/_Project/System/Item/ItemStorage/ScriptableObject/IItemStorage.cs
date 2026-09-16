using System;
using UnityEngine;
/// <summary>
/// Defines a interface that can hold Storage Slots
/// Handles contracts for handling items in the Storage Slots
/// </summary>
public interface IItemStorage
{

	int StorageCapacity { get; }

	bool CanDropToWorld { get; }
	event Action<int> OnSlotUpdated;

	/// <summary>
	/// Gets the item at the given index
	/// </summary>
	ItemInstance GetItem(int index);

	/// <summary>
	/// Sets the item at the given index
	/// </summary>
	void SetItem(int index, ItemInstance item);

	/// <summary>
	/// Swaps the items at the given indices
	/// </summary>
	void SwapItems(int indexA, int indexB);

	/// <summary>
	/// Refreshes the item data at the given index
	/// </summary>
	void RefreshSlot(int index);

	/// <summary>
	/// Drops the item at the given index to the world
	/// </summary>
	void DropItems(int index, Vector3 spawnPosition);
}
