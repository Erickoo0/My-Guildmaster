using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HotbarSlotUI : MonoBehaviour, IStorageSlot
{
	public int slotScriptIndex;

	[Header("UI References")]
	[SerializeField] private Image itemIconDisplay;
	[SerializeField] private TextMeshProUGUI itemNameText;
	[SerializeField] private TextMeshProUGUI itemStackText;

	private bool _isBeingDragged = false;

	private void Update()
	{
		if (itemInstance?.DataSo == null || _isBeingDragged)
			return;

		itemIconDisplay.sprite = GlobalHelper.GetAnimatedSprite(itemInstance.DataSo);
	}
	public int Index => slotScriptIndex;
	public ItemInstance itemInstance => InventoryManager.Instance.itemsList[slotScriptIndex];

	public void RefreshSlotUI()
	{
		var item = itemInstance;                            // Gets data from Inventory Manager via Property
		bool hasItem = item != null && item.DataSo != null; // Check if the slot has an item
		bool shouldShow = hasItem && !_isBeingDragged;      // Hides elements while being dragged

		// Set the text
		itemNameText.text = hasItem ? item.DataSo.ItemName : null;
		itemStackText.text = hasItem ? item.stackSize.ToString() : null;
		itemIconDisplay.sprite = hasItem ? itemInstance.DataSo.ItemIcon[0] : null;
		itemIconDisplay.enabled = shouldShow;
	}

	public void SetDraggingState(bool isDragging)
	{
		_isBeingDragged = isDragging;
		RefreshSlotUI();
	}
}
