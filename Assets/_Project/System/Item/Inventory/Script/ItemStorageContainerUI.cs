using System.Collections.Generic;
using UnityEngine;
public class ItemStorageContainerUI : MonoBehaviour
{
	[SerializeField] private GameObject _itemContainerMenu;
	[SerializeField] private GameObject _storageSlotUIPrefab;
	[SerializeField] private Transform _storageSlotContainer;

	private readonly List<IItemSlotUI> _storageSlotList = new List<IItemSlotUI>();
	private IItemStorage _currentContainer;

	private void OnEnable()
	{
		EventBus.OnMenuClosed += HandleMenuClosed;
		EventBus.OnStorageOpenRequested += OpenStorage;
	}

	private void OnDisable()
	{
		EventBus.OnMenuClosed -= HandleMenuClosed;
		EventBus.OnStorageOpenRequested -= OpenStorage;

		if (_currentContainer != null)
			_currentContainer.OnSlotUpdated -= RefreshSlot;
	}

	public void OpenStorage(IItemStorage storage)
	{
		if (storage == null)
			return;

		// Clean up old subscriptions in case we force-open a new chest while one is already open
		if (_currentContainer != null)
			_currentContainer.OnSlotUpdated -= RefreshSlot;

		_currentContainer = storage;
		_currentContainer.OnSlotUpdated += RefreshSlot;

		BuildSlots();
		RefreshAllSlots();

		if (!_itemContainerMenu.activeSelf)
			EventBus.RequestOpenMenu(_itemContainerMenu);
	}

	public void RefreshSlot(int index)
	{
		if (index < 0 || index >= _storageSlotList.Count) return;
		_storageSlotList[index].RefreshSlotUI();
	}

	private void BuildSlots()
	{
		// Safety Check
		if (_currentContainer == null || _storageSlotContainer == null || _storageSlotUIPrefab == null)
			return;

		// 1. Check if the capacity has changed
		bool needsRebuild = _storageSlotList.Count != _currentContainer.StorageCapacity;

		if (needsRebuild)
		{
			// 2. Destroy old slots
			for (int i = _storageSlotContainer.childCount - 1; i >= 0; i--)
				Destroy(_storageSlotContainer.GetChild(i).gameObject);

			_storageSlotList.Clear();


			// 3. Create new slots
			for (int i = 0; i < _currentContainer.StorageCapacity; i++)
			{
				GameObject slot = Instantiate(_storageSlotUIPrefab, _storageSlotContainer);

				if (slot.TryGetComponent(out ItemSlotUI storageSlotUI))
				{
					storageSlotUI.Setup(_currentContainer, i);
					_storageSlotList.Add(storageSlotUI);
				}
			}
		}

		// 4. Refresh the items
		for (int i = 0; i < _currentContainer.StorageCapacity; i++)
			_storageSlotList[i].Setup(_currentContainer, i);

	}

	private void RefreshAllSlots()
	{
		for (int i = 0; i < _storageSlotList.Count; i++)
			_storageSlotList[i].RefreshSlotUI();
	}

	private void HandleMenuClosed(GameObject closedMenu)
	{
		if (closedMenu != _itemContainerMenu) return;
		_currentContainer = null;
	}
}
