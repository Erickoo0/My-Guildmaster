using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Represents an object that holds items.
/// Handles item storage, addition, and swapping, while notifying listeners of slot changes via events.
/// Can be opened or destroyed to drop items.
/// </summary>
[RequireComponent(typeof(UniqueIdentifier))]
public class ItemStorageContainer : MonoBehaviour, IInteractable, ISaveable, IItemStorage
{
	[Header("References")]
	[SerializeField] private Sprite _containerOpenedSprite;

	[Header("Interaction Settings")]
	[SerializeField] private bool _interactable = true;

	[Header("Storage Settings")]
	[SerializeField] private List<ItemDrop> _defaultItemsList = new List<ItemDrop>();
	[SerializeField] private int _storageSize = 10;

	[Header("Drop Settings")]
	[SerializeField] private float _spreadRadius = 1.5f;


	// Cached components and data
	private Health _health;
	private SpriteRenderer _spriteRenderer;
	private ItemInstance[] _storedItemsList;
	private UniqueIdentifier _uniqueID;

	[Header("Save Data")]
	public bool IsOpened { get; private set; }

	private void Awake()
	{
		_health = GetComponent<Health>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_uniqueID = GetComponent<UniqueIdentifier>();

		Setup();
	}

	private void OnEnable()
	{
		if (_health != null)
			_health.OnDeath += BreakContainer;
	}

	private void OnDisable()
	{
		if (_health != null)
			_health.OnDeath -= BreakContainer;
	}

	//---- IInteractable Logic ----

	public bool CanInteract() => _interactable;

	/// <summary>
	/// Opens the container when the player interacts with it.
	/// </summary>
	public void Interact(ControllerPlayer controllerPlayer = null)
	{
		if (!CanInteract()) return;

		if (!IsOpened)
			UpdateVisuals();

		EventBus.RequestOpenStorage(this);
	}
	public int StorageCapacity => _storageSize;

	public event Action<int> OnSlotUpdated;
	public bool CanDropToWorld => false; // Prevent dropping items to the ground directly from chest UI

	//---- IItemStorage Logic ----

	/// <summary>
	/// Retrieves the ItemInstance at the specified storage slot index.
	/// </summary>
	public ItemInstance GetItem(int index)
	{
		if (_storedItemsList == null || index < 0 || index >= _storedItemsList.Length) return null;
		return _storedItemsList[index];
	}

	/// <summary>
	/// Sets an ItemInstance in the specified storage slot.
	/// </summary>
	public void SetItem(int index, ItemInstance item)
	{
		if (_storedItemsList == null || index < 0 || index >= _storedItemsList.Length) return;
		_storedItemsList[index] = item;
		RefreshSlot(index);
	}

	/// <summary>
	/// Swap items from specified indices 
	/// </summary>
	public void SwapItems(int indexA, int indexB)
	{
		if (_storedItemsList == null) return;
		if (indexA < 0 || indexA >= _storedItemsList.Length || indexB < 0 || indexB >= _storedItemsList.Length) return;

		(_storedItemsList[indexA], _storedItemsList[indexB]) = (_storedItemsList[indexB], _storedItemsList[indexA]);
		RefreshSlot(indexA);
		RefreshSlot(indexB);
	}

	/// <summary>
	/// Drops an item from the specified inventory slot into the game world at the given position.
	/// </summary>
	public void DropItems(int index, Vector3 spawnPosition)
	{
		if (_storedItemsList == null || index < 0 || index >= _storedItemsList.Length) return;
		if (_storedItemsList[index] == null || _storedItemsList[index].DataSo == null) return;

		GameObject prefabToSpawn = _storedItemsList[index].DataSo.ItemObject != null
			? _storedItemsList[index].DataSo.ItemObject
			: ItemStoragePlayer.Instance.DefaultItemObjectPrefab;

		GameObject droppedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

		if (droppedItem.TryGetComponent(out ItemObject itemObject))
			itemObject.SetItemObject(_storedItemsList[index]);

		_storedItemsList[index] = null;
		RefreshSlot(index);
	}

	/// <summary>
	/// Refreshes the item at the specified inventory slot.
	/// </summary>
	public void RefreshSlot(int index)
	{
		OnSlotUpdated?.Invoke(index);
	}

	//---- Load & Save Data Logic ----

	/// <summary>
	/// Stores the containers opened state in the save data.
	/// </summary>
	public void PopulateSaveData(SaveData saveData)
	{
		if (IsOpened)
		{
			// Check if the ChestsOpenedList save data already contains this chest id
			if (!saveData.ChestsOpenedList.Contains(GetID()))
			{
				saveData.ChestsOpenedList.Add(GetID());
			}
		}
	}

	/// <summary>
	/// Restores the containers opened state from the saved data.
	/// </summary>
	public void LoadFromSaveData(SaveData saveData)
	{
		if (saveData.ChestsOpenedList.Contains(GetID()))
		{
			IsOpened = true;
			UpdateVisuals();
		} else
		{
			IsOpened = false;
		}
	}

	private void Setup()
	{
		// Get the storage capacity
		int slotCount = Mathf.Max(_defaultItemsList.Count, _storageSize);

		// Create the Item Storage list
		_storedItemsList = new ItemInstance[slotCount];

		// Add the default items to the Item Storage list
		for (int i = 0; i < _defaultItemsList.Count; i++)
		{
			if (_defaultItemsList[i].itemDataSo == null || _defaultItemsList[i].dropAmount <= 0)
				continue;

			_storedItemsList[i] = new ItemInstance(_defaultItemsList[i].itemDataSo, _defaultItemsList[i].dropAmount);
		}
	}

	private void UpdateVisuals()
	{
		IsOpened = true;
		if (_spriteRenderer != null && _containerOpenedSprite != null)
			_spriteRenderer.sprite = _containerOpenedSprite;
	}

	//---- Destruction & Loot Logic ----

	/// <summary>
	/// Called when the container is destroyed by health reaching 0.
	/// </summary>
	public void BreakContainer()
	{
		if (!IsOpened)
			UpdateVisuals();

		DropAllItems();
	}

	/// <summary>
	/// Spills all contained items into the game world.
	/// </summary>
	private void DropAllItems()
	{
		// Filter out null data and add valid data to list
		var validDrops = _storedItemsList.Where(d => d != null && d.DataSo != null && d.stackSize > 0).ToList();
		if (validDrops.Count <= 0) return;

		// 1. Calculate the angle step for even distribution
		float angleStep = 360f/validDrops.Count;
		float currentAngle = Random.Range(0f, 360f);

		foreach (ItemInstance drop in validDrops)
		{
			// 2. Add random offset of up to 40%
			float angleOffset = Random.Range(-angleStep*0.4f, angleStep*0.4f);
			float finalAngle = currentAngle + angleOffset;

			SpawnItem(drop, finalAngle);
			currentAngle += angleStep;
		}

		// 3. Clear out the array entirely
		for (int i = 0; i < _storedItemsList.Length; i++)
		{
			_storedItemsList[i] = null;
			RefreshSlot(i);
		}
	}

	private void SpawnItem(ItemInstance drop, float angle)
	{
		// 1. Get the Default ItemObject prefab from ItemStoragePlayer
		GameObject prefabToSpawn = drop.DataSo.ItemObject != null
			? drop.DataSo.ItemObject
			: ItemStoragePlayer.Instance.DefaultItemObjectPrefab;

		Vector3 targetPosition = CalculateDropPosition(angle);
		GameObject droppedItem = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

		if (droppedItem.TryGetComponent(out ItemObject itemObject))
		{
			var instance = new ItemInstance(drop.DataSo, drop.stackSize);
			itemObject.SetItemObject(instance, targetPosition);
		}
	}

	/// <summary>
	/// Calculates the position of the item drop based on the angle and spread radius.
	/// </summary>
	public Vector3 CalculateDropPosition(float angleDegrees)
	{
		// 1. Convert angle from degrees to radian
		float angleRad = angleDegrees*Mathf.Deg2Rad;

		// 2. Get the normalized x and y direction
		Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

		// 3. Pick a distance using Square Root distribution. 
		// This prevents items from artificially clumping in the center of the circle.
		float randomValue = Mathf.Sqrt(Random.value);
		float distance = Mathf.Lerp(0.5f, _spreadRadius, randomValue);

		// 4. Return the final position
		// Offset Y slightly to look better in 2D top-down perspective
		return transform.position + new Vector3(direction.x*distance, (direction.y*distance) - 0.5f, 0);
	}

	private string GetID() => _uniqueID.ID;

	//---- Helper Structures ----

	/// <summary>
	/// Struct to hold ItemDataSo and amount to drop.
	/// </summary>
	[Serializable]
	public struct ItemDrop
	{
		public ItemDataSo itemDataSo;
		public int dropAmount;
	}
}
