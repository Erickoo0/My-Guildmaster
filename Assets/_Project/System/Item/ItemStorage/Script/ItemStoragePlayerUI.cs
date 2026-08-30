using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Manages the visual representation of the player's inventory. 
/// Spawns <see cref="ItemSlotUI"/> elements and listens for data changes to refresh specific slots.
/// </summary>
public class ItemStoragePlayerUI : MonoBehaviour
{
	[SerializeField] private GameObject _storageSlotUIPrefab; // Inventory slot to spawn
	[SerializeField] private GameObject _inventoryMenuPanel;
	[SerializeField] private GameObject _inventorySlotContainer;
	[SerializeField] private Image _inventoryPortrait;
	[SerializeField] private GameObject _hotbarSlotContainer;
	[SerializeField] private int _hotbarSize = 10;

	[Header("Selection Frame Settings")]
	[SerializeField] private RectTransform _slotSelectionFrame;
	[SerializeField] private float _lerpSpeed = 15f;

	private readonly List<IItemSlotUI> _storageSlotsList = new List<IItemSlotUI>();

	private SpriteRenderer _playerSpriteRenderer;
	private Vector3 _targetPosition;

	private void Start()
	{
		SetupUI();

		// Cache the portrait components
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		var visualComponent = player.transform.Find("Visual");
		_playerSpriteRenderer = visualComponent.GetComponent<SpriteRenderer>();

		ItemStoragePlayer.Instance.OnSlotUpdated += RefreshSlotUI;
		ItemStoragePlayer.Instance.OnActiveSlotIndexChanged += MoveSelectionFrame;

		// Initial Refresh: Sync UI with whatever data is already in the Inventory Manager (Saved data)
		for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			RefreshSlotUI(i);

		_slotSelectionFrame.gameObject.SetActive(true);
	}


	private void Update()
	{
		// Moves selection frame to target position
		_slotSelectionFrame.position = Vector3.Lerp(_slotSelectionFrame.position, _targetPosition, Time.deltaTime*_lerpSpeed);

		// Set the portrait to the player
		if (_inventoryPortrait != null && _playerSpriteRenderer != null)
			_inventoryPortrait.sprite = _playerSpriteRenderer.sprite;

	}

	private void OnDestroy()
	{
		ItemStoragePlayer.Instance.OnSlotUpdated -= RefreshSlotUI;
		ItemStoragePlayer.Instance.OnActiveSlotIndexChanged -= MoveSelectionFrame;
	}

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (!_inventoryMenuPanel.activeSelf) EventBus.RequestOpenMenu(_inventoryMenuPanel);
		else if (_inventoryMenuPanel.activeSelf) EventBus.RequestCloseMenu(_inventoryMenuPanel);
	}

	private void SetupUI()
	{
		int totalSlots = ItemStoragePlayer.Instance.StorageCapacity;

		for (int i = 0; i < totalSlots; i++)
		{
			// Determine if this slot should go to the hotbar or the main inventory container
			GameObject targetParent = (i < _hotbarSize) ? _hotbarSlotContainer : _inventorySlotContainer;

			// Instantiate the slot
			GameObject slot = Instantiate(_storageSlotUIPrefab, targetParent.transform);

			// Setup the storage slot and add it to the list
			if (slot.TryGetComponent(out ItemSlotUI storageSlot))
			{
				storageSlot.Setup(ItemStoragePlayer.Instance, i);
				_storageSlotsList.Add(storageSlot);
			}
		}
	}

	private void RefreshSlotUI(int index)
	{
		if (index >= 0 && index < _storageSlotsList.Count)
		{
			_storageSlotsList[index].RefreshSlotUI();
		}
	}

	public void MoveSelectionFrame(int index)
	{
		// Checks if _storageSlotsList[SlotIndex] is an ItemSlotUI, if true, assigns it to slotBase
		if (_storageSlotsList[index] is ItemSlotUI slotBase)
		{
			_targetPosition = slotBase.transform.position;
		}
	}
}
