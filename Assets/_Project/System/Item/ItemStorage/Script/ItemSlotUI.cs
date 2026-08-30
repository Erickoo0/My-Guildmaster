using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Defines a single slot UI element that holds an item
/// </summary>
public class ItemSlotUI : MonoBehaviour, IItemSlotUI
{

	[Header("UI References")]
	[SerializeField] private Image _itemIcon;
	[SerializeField] private TextMeshProUGUI _itemName;
	[SerializeField] private TextMeshProUGUI _itemStack;

	private bool _isBeingDragged;

	private void Update()
	{
		// If the item is not animated or is being dragged, return
		if (itemInstance?.DataSo == null || !itemInstance.DataSo.IsAnimated || _isBeingDragged)
			return;

		// Update the icon for animated sprites
		_itemIcon.sprite = GlobalHelper.GetAnimatedSprite(itemInstance.DataSo);
	}
	public int SlotIndex { get; private set; }            // The index of the slot
	public IItemStorage ItemStorage { get; private set; } // The storage container the slot belongs to
	public ItemInstance itemInstance => ItemStorage?.GetItem(SlotIndex);

	/// <summary>
	/// Storage UI Manager initializes the slot with the storage and index.
	/// </summary>
	public void Setup(IItemStorage storage, int index)
	{
		ItemStorage = storage;
		SlotIndex = index;
		RefreshSlotUI();
	}

	/// <summary>
	/// Updates the UI elements based on the current item in the slot.
	/// </summary>
	public void RefreshSlotUI()
	{
		var item = itemInstance;
		bool hasItem = item != null && item.DataSo != null;

		// Hides elements while being dragged
		bool shouldShow = hasItem && !_isBeingDragged;

		// Set the text
		_itemName.text = hasItem ? item.DataSo.ItemName : null;
		_itemStack.text = hasItem ? item.stackSize.ToString() : null;

		// Set the sprite
		_itemIcon.sprite = hasItem ? itemInstance.DataSo.ItemIcon[0] : null;
		_itemIcon.enabled = shouldShow;
	}

	public void SetDraggingState(bool isDragging)
	{
		_isBeingDragged = isDragging;
		RefreshSlotUI();
	}
}
