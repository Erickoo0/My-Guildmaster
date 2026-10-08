using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Handles displaying the crafting system ui and crafting items
/// </summary>
public class CraftingStationUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _menuPanel;
	[SerializeField] private GameObject _recipeSlotPrefab;
	[SerializeField] private Transform _recipeSlotContainer;
	[SerializeField] private GameObject _reosurceSlotPrefab;
	[SerializeField] private Transform _resourceSlotContainer;

	[Header("Item Detail & Crafting")]
	[SerializeField] private TextMeshProUGUI _itemName;
	[SerializeField] private TextMeshProUGUI _itemDescription;
	[SerializeField] private Image _itemIcon;
	[SerializeField] private Button _craftButton;
	private List<GameObject> _activeResourceSlotsList = new List<GameObject>();

	private List<CraftingRecipeSlotUI> _recipeSlotsList = new List<CraftingRecipeSlotUI>();
	private ItemDataSo _selectedRecipe;

	private void Start()
	{
		EventBus.OnCraftingRecipeUnlocked += CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryUpdated; // To turn the craft button on/off
		_craftButton.onClick.AddListener(OnCraftButtonClicked);

		// 1. Spawn all initial unlocked recipeSlotUI
		foreach (ItemDataSo recipe in CraftingRecipeManager.Instance.UnlockedRecipesList)
			CreateRecipeSlotUI(recipe);

		// 2. Clear item details
		ClearItemDetails();
	}

	private void OnDestroy()
	{
		EventBus.OnCraftingRecipeUnlocked -= CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryUpdated;
	}

	/// <summary>
	/// Create a new recipe slot UI for the given recipe
	/// </summary>
	private void CreateRecipeSlotUI(ItemDataSo newRecipe)
	{
		// 1. Create the recipeSlotUI
		GameObject newRecipeSlot = Instantiate(_recipeSlotPrefab, _recipeSlotContainer);

		// 2. Get the property and assign the data
		if (newRecipeSlot.TryGetComponent(out CraftingRecipeSlotUI slotUI))
		{
			slotUI.Setup(newRecipe);

			// Listen for when this specific recipe slot is clicked
			slotUI.OnRecipeSelected += HandleRecipeSelected;

			// 3. Add to the list
			_recipeSlotsList.Add(slotUI);
		}
	}

	// Used to dispose the event variable
	private void HandleInventoryUpdated(int _) => RefreshItemDetails();

	private void HandleRecipeSelected(CraftingRecipeSlotUI selectedSlot, ItemDataSo recipe)
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
		_itemIcon.color = Color.white;

		// 1. Clear old resource slot prefabs
		foreach (GameObject resourceSlotUI in _activeResourceSlotsList)
			Destroy(resourceSlotUI);
		_activeResourceSlotsList.Clear();

		// 2. Safety Check
		if (_selectedRecipe == null || !_selectedRecipe.TryGetProperty(out ItemPropertyCraftingRecipe recipe))
		{
			_craftButton.interactable = false;
			return;
		}

		// 3. Spawn new resouece slot prefabs and pass them the inventory counts
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
			GameObject resourceSlotUI = Instantiate(_reosurceSlotPrefab, _resourceSlotContainer);
			_activeResourceSlotsList.Add(resourceSlotUI);

			// 6. Pass the data to the prefab
			if (resourceSlotUI.TryGetComponent(out CraftingResourceSlotUI resourceSlot))
				resourceSlot.Setup(requirement.ItemDataSo, requirement.Amount, totalFound);

			// 7. Refresh the craft button
			_craftButton.interactable = CraftingStationManager.Instance.HasResources(recipe);
		}
	}

	private void ClearItemDetails()
	{
		_selectedRecipe = null;
		_itemIcon.color = Color.clear;
		_itemName.text = "";
		_itemDescription.text = "";
		_craftButton.interactable = false;
	}

	private void OnCraftButtonClicked()
	{
		if (_selectedRecipe != null)
			EventBus.RequestCraftItem(_selectedRecipe);
	}

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed)
			return;

		if (!_menuPanel.activeSelf)
			EventBus.RequestOpenMenu(_menuPanel);
		else
			EventBus.RequestCloseMenu(_menuPanel);
	}
}
