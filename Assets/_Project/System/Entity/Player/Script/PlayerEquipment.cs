using UnityEngine;
/// <summary>
/// Manages the visual representation and mechanical execution of the player's active item.
/// Spawns the physical item prefab in the game world and handle the use function.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{

	[Header("Equipment Settings")]
	[SerializeField] private Transform parentTransform;

	private GameObject _currentActiveItem;
	private int _currentActiveSlotIndex = 0;
	public static PlayerEquipment Instance { get; private set; }

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			Debug.unityLogger.Log("Multiple PlayerEquipment detected. Disabling script.");
			return;
		}

		Instance = this;
	}

	private void Start()
	{
		ItemStoragePlayer.Instance.OnActiveSlotIndexChanged += SetActiveSlotIndex;
		ItemStoragePlayer.Instance.OnSlotUpdated += SetSlotData;
	}

	private void OnDestroy()
	{
		if (ItemStoragePlayer.Instance != null)
		{
			ItemStoragePlayer.Instance.OnActiveSlotIndexChanged -= SetActiveSlotIndex;
			ItemStoragePlayer.Instance.OnSlotUpdated -= SetSlotData;
		}

	}

	private void SetActiveSlotIndex(int index)
	{
		parentTransform.gameObject.SetActive(true);
		_currentActiveSlotIndex = index;
		// Find the Item Data from slot index
		ItemInstance item = ItemStoragePlayer.Instance.GetItem(_currentActiveSlotIndex);
		// Set the active item to the one from slot index
		SetActiveItem(item);
	}

	private void SetSlotData(int index)
	{
		// Only update if the slot modified matches active slot
		if (index == _currentActiveSlotIndex)
		{
			ItemInstance item = ItemStoragePlayer.Instance.GetItem(_currentActiveSlotIndex);
			SetActiveItem(item);
		}
	}

	private void SetActiveItem(ItemInstance itemInSlot)
	{
		// Destroy the old active item if it exists
		if (_currentActiveItem != null) Destroy(_currentActiveItem);

		// Safety Check: If slot is empty or null
		if (itemInSlot == null || itemInSlot.DataSo == null || itemInSlot.DataSo.ItemObject == null) return;

		// Spawn the Item Object
		_currentActiveItem = Instantiate(itemInSlot.DataSo.ItemObject, parentTransform);

		// Reset position
		_currentActiveItem.transform.localPosition = Vector3.zero;
		_currentActiveItem.transform.localRotation = Quaternion.identity;

		// Setup the active items sprite and data
		if (_currentActiveItem.TryGetComponent(out ItemObject itemObjectScript))
		{
			itemObjectScript.SetItemObject(itemInSlot, null, false);
		}
	}

	public void TryUseActiveItem()
	{
		if (_currentActiveSlotIndex < 0)
			return;

		// 1. Set the active item to the one from slot index
		ItemInstance activeItem = ItemStoragePlayer.Instance.GetItem(_currentActiveSlotIndex);

		if (activeItem == null || activeItem.DataSo == null)
		{
			Debug.LogWarning($"TryUseActiveItem: Slot {_currentActiveSlotIndex} is empty or missing DataSo.");
			return;
		}

		// 2. Check if the item is usable
		if (activeItem.DataSo.IsUsable)
		{
			// 3. Execute the use function of the item and remove it from inventory
			bool wasUsed = activeItem.DataSo.Use(activeItem, gameObject);
			Debug.Log($"Item used: {wasUsed}");
			if (wasUsed)
				ItemStoragePlayer.Instance.RemoveItems(_currentActiveSlotIndex);
		} else
		{
			Debug.LogWarning($"TryUseActiveItem: item {activeItem.DataSo.ItemName} is not usable");
		}
	}
}
