using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Handles displaying the crafting system ui
/// </summary>
public class ItemCraftingMenuUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _craftingMenuPanel;
	[SerializeField] private GameObject _itemRecipeUIPrefab;
	[SerializeField] private Transform _itemRecipeContainer;

	private List<ItemRecipeUI> _itemRecipeSlotsList = new List<ItemRecipeUI>();

	private void Start()
	{
		EventBus.OnRecipeUnlocked += SpawnItemRecipeSlot;
		ItemStoragePlayer.Instance.OnSlotUpdated += RefreshAllSlots; // To turn the craft button on/off

		// 1. Spawn all initial unlocked recipe slots
		foreach (ItemDataSo recipe in RecipeManager.Instance.UnlockedRecipesList)
			SpawnItemRecipeSlot(recipe);
	}

	private void OnDestroy()
	{
		EventBus.OnRecipeUnlocked -= SpawnItemRecipeSlot;
		ItemStoragePlayer.Instance.OnSlotUpdated -= RefreshAllSlots;
	}

	/// <summary>
	/// Spawn a new item recipe slot for the given recipe
	/// </summary>
	private void SpawnItemRecipeSlot(ItemDataSo newRecipe)
	{
		// 1. Spawn the prefab
		GameObject itemRecipeUIObject = Instantiate(_itemRecipeUIPrefab, _itemRecipeContainer);

		// 2. Get the property and assign the data
		if (itemRecipeUIObject.TryGetComponent(out ItemRecipeUI slotUI))
		{
			slotUI.Setup(newRecipe);
			slotUI.RefreshState();

			// 3. Add to the list
			_itemRecipeSlotsList.Add(slotUI);
		}
	}

	/// <summary>
	/// Loop through all item recipe slots and refresh their state
	/// </summary>
	private void RefreshAllSlots(int slotIndex)
	{
		foreach (ItemRecipeUI slotUI in _itemRecipeSlotsList)
			slotUI.RefreshState();
	}

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed)
			return;

		if (!_craftingMenuPanel.activeSelf)
			EventBus.RequestOpenMenu(_craftingMenuPanel);
		else
			EventBus.RequestCloseMenu(_craftingMenuPanel);
	}
}
