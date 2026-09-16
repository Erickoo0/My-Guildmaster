using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Handles displaying the crafting system ui and crafting items
/// </summary>
public class ItemCraftingMenuUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _craftingMenuPanel;
	[SerializeField] private GameObject _itemRecipeUIPrefab;
	[SerializeField] private Transform _itemRecipeContainer;
	[SerializeField] private GameObject _craftingResourcePrefab;
	[SerializeField] private Transform _craftingResourceContainer;

	[Header("Item Detail & Crafting")]
	[SerializeField] private TextMeshProUGUI _itemName;
	[SerializeField] private TextMeshProUGUI _itemDescription;
	[SerializeField] private Image _itemIcon;
	[SerializeField] private Button _craftButton;
	private List<GameObject> _activeCraftingResourcesList = new List<GameObject>();

	private List<ItemRecipeSlotUI> _itemRecipeSlotsList = new List<ItemRecipeSlotUI>();
	private ItemDataSo _selectedRecipe;

	private void Start()
	{
		EventBus.OnRecipeUnlocked += SpawnItemRecipeSlot;
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryUpdated; // To turn the craft button on/off
		_craftButton.onClick.AddListener(OnCraftButtonClicked);

		// 1. Spawn all initial unlocked recipe slots
		foreach (ItemDataSo recipe in RecipeManager.Instance.UnlockedRecipesList)
			SpawnItemRecipeSlot(recipe);

		// 2. Clear item details
		ClearTooltip();
	}

	private void OnDestroy()
	{
		EventBus.OnRecipeUnlocked -= SpawnItemRecipeSlot;
		ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryUpdated;
	}

	/// <summary>
	/// Spawn a new item recipe slot for the given recipe
	/// </summary>
	private void SpawnItemRecipeSlot(ItemDataSo newRecipe)
	{
		// 1. Spawn the prefab
		GameObject itemRecipeUIObject = Instantiate(_itemRecipeUIPrefab, _itemRecipeContainer);

		// 2. Get the property and assign the data
		if (itemRecipeUIObject.TryGetComponent(out ItemRecipeSlotUI slotUI))
		{
			slotUI.Setup(newRecipe);

			// Listen for when this specific recipe slot is clicked
			slotUI.OnRecipeSelected += HandleRecipeSelected;

			// 3. Add to the list
			_itemRecipeSlotsList.Add(slotUI);
		}
	}

	// Used to dispose the event variable
	private void HandleInventoryUpdated(int _) => RefreshItemDetails();

	private void HandleRecipeSelected(ItemRecipeSlotUI selectedSlot, ItemDataSo recipe)
	{
		_selectedRecipe = recipe;

		// 1. Update item details
		_itemName.text = recipe.ItemName;
		_itemDescription.text = recipe.ItemDescription;
		_itemIcon.sprite = recipe.ItemIcon[0];

		// 2. Evaluate if we can press the craft button
		RefreshItemDetails();
	}

	private void RefreshItemDetails()
	{
		// 1. Clear old crafting resource prefabs
		foreach (GameObject craftingResource in _activeCraftingResourcesList)
			Destroy(craftingResource);

		_activeCraftingResourcesList.Clear();

		// 2. Safety Check
		if (_selectedRecipe == null || !_selectedRecipe.TryGetProperty(out ItemPropertyCraftingRecipe recipe))
		{
			_craftButton.interactable = false;
			return;
		}

		// 3. Spawn new crafting resource prefabs and pass them the inventory counts
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requirement in recipe.RequiredResourcesList)
		{
			int totalFound = 0;

			// 4. Get the total ingredients found in inventory
			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			{
				ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);
				if (item != null && item.DataSo == requirement.ItemDataSo)
					totalFound += item.stackSize;
			}

			// 5. Create the prefab and add it to the list
			GameObject craftingResourceSlot = Instantiate(_craftingResourcePrefab, _craftingResourceContainer);
			_activeCraftingResourcesList.Add(craftingResourceSlot);

			// 6. Pass the data to the prefab
			if (craftingResourceSlot.TryGetComponent(out CraftingResourceSlotUI craftingResourceSlotUI))
				craftingResourceSlotUI.Setup(requirement.ItemDataSo, requirement.Amount, totalFound);

			// 7. Refresh the craft button
			_craftButton.interactable = CraftingManager.Instance.HasResources(recipe);
		}
	}

	private void OnCraftButtonClicked()
	{
		if (_selectedRecipe != null)
			EventBus.RequestCraftItem(_selectedRecipe);
	}

	private void ClearTooltip()
	{
		_selectedRecipe = null;
		_itemName.text = "";
		_itemDescription.text = "";
		_craftButton.interactable = false;
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
