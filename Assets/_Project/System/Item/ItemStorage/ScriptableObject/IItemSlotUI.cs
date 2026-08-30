/// <summary>
/// Defines a interface that handles the data and UI of a Storage Slot which holds items
/// </summary>
public interface IItemSlotUI
{
	/// <summary>
	/// The IItemStorage that this slot belongs to
	/// </summary>
	IItemStorage ItemStorage { get; }

	/// <summary>
	/// The ItemInstance that this slot is displaying
	/// </summary>
	ItemInstance itemInstance { get; }

	/// <summary>
	/// The index of this slot in the IItemStorage
	/// </summary>
	int SlotIndex { get; }

	void Setup(IItemStorage storage, int index);

	/// <summary>
	/// Refreshes the UI for this slot
	/// </summary>
	void RefreshSlotUI();

	/// <summary>
	/// Sets the dragging state of this slot
	/// </summary>
	void SetDraggingState(bool isDragging);
}
