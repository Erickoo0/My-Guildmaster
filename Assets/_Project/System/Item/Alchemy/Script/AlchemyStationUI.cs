using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles displaying the Alchemy Station UI and brewing items
/// </summary>
public class AlchemyStationUI : MonoBehaviour
{
	[Header("Reference")]
	[SerializeField] private GameObject _menuPanel;
	[SerializeField] private GameObject _recipeSlotPrefab;
	[SerializeField] private Transform _recipeSlotContainer;
	[SerializeField] private GameObject _resourceSlotPrefab;
	[SerializeField] private Transform _resourceSlotContainer;

	[Header("Item Detail & Brewing")]
	[SerializeField] private TextMeshProUGUI _itemName;
	[SerializeField] private TextMeshProUGUI _itemDescription;
	[SerializeField] private Image _itemIcon;
	[SerializeField] private Button _brewButton;
	private readonly MenuType menuType = MenuType.Alchemy;
	private List<GameObject> _activeResourceSlotsList = new List<GameObject>();

	private List<AlchemyRecipeSlotUI> _recipeSlotsList = new List<AlchemyRecipeSlotUI>();
	private ItemDataSo _selectedRecipe;

	private void Start()
	{
		EventBus.OnMenuToggleRequested += HandleMenuToggle;
		EventBus.OnAlchemyBrewRequested += CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryUpdated;
		_brewButton.onClick.AddListener(OnBrewButtonClicked);

		// 1. Create all initial unlocked recipeSlotUI
		foreach (ItemDataSo recipe in AlchemyRecipeManager.Instance.UnlockedRecipesList)
			CreateRecipeSlotUI(recipe);

		// 2. Clear item details
		ClearItemDetails();
	}

	private void OnDestroy()
	{
		EventBus.OnMenuToggleRequested -= HandleMenuToggle;
		EventBus.OnAlchemyBrewRequested -= CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryUpdated;
	}

	/// <summary>
	/// Create a new recipe slot UI for the given recipe
	/// </summary>
	/// <param name="newRecipe"></param>
	private void CreateRecipeSlotUI(ItemDataSo newRecipe)
	{
		// 1. Create the recipeSlotUI
		GameObject newRecipeSlot = Instantiate(_recipeSlotPrefab, _recipeSlotContainer);

		// 2. Get the property and assign the data
		if (newRecipeSlot.TryGetComponent<AlchemyRecipeSlotUI>(out AlchemyRecipeSlotUI slotUI))
		{
			slotUI.Setup(newRecipe);

			// Listen for when this specific recipe slot is clicked
			slotUI.OnRecipeSelected += HandleRecipeSelected;

			// 3. Add to the list
			_recipeSlotsList.Add(slotUI);
		}
	}

	private void HandleInventoryUpdated(int _) => RefreshItemDetails();

	private void HandleRecipeSelected(AlchemyRecipeSlotUI selectedSlot, ItemDataSo selectedRecipe)
	{
		// 1. Update item details
		_selectedRecipe = selectedRecipe;
		_itemName.text = selectedRecipe.ItemName;
		_itemDescription.text = selectedRecipe.ItemDescription;
		_itemIcon.sprite = selectedRecipe.ItemIcon[0];

		// 2. Evaluate if we can press the brew button
		RefreshItemDetails();
	}

	private void RefreshItemDetails()
	{
		// 1. Clear old resource slot prefabs
		foreach (GameObject resourceSlotUI in _activeResourceSlotsList)
			Destroy(resourceSlotUI.gameObject);
		_activeResourceSlotsList.Clear();

		/// 2. Safety Check
		if (_selectedRecipe == null || !_selectedRecipe.TryGetProperty(out ItemPropertyAlchemyRecipe recipe))
		{
			_brewButton.interactable = false;
			return;
		}

		// 3. Spawn new resource slot prefabs and pass them the inventory counts
		foreach (ItemPropertyAlchemyRecipe.ResourceRequirement requirement in recipe.RequiredResourcesList)
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
			GameObject resourceSlotUI = Instantiate(_resourceSlotPrefab, _resourceSlotContainer);
			_activeResourceSlotsList.Add(resourceSlotUI);

			// 6. Pass the data to the prefab
			if (resourceSlotUI.TryGetComponent(out AlchemyResourceSlotUI resourceSlot))
				resourceSlot.Setup(requirement.ItemDataSo, requirement.Amount, totalFound);

			// 7. Refresh the craft button
			_brewButton.interactable = AlchemyStationManager.Instance.HasResources(recipe);
		}
	}

	private void ClearItemDetails()
	{
		_selectedRecipe = null;
		_itemName.text = "";
		_itemDescription.text = "";
		_brewButton.interactable = false;
	}

	private void OnBrewButtonClicked()
	{
		if (_selectedRecipe != null)
			EventBus.RequestAlchemyBrew(_selectedRecipe);
	}

	public void HandleMenuToggle(MenuType requestedMenu)
	{
		// 1. Check ift he requested menu matches this UI menu
		if (requestedMenu != menuType || _menuPanel == null)
			return;

		if (!_menuPanel.activeSelf)
			EventBus.RequestOpenMenu(_menuPanel);
		else
			EventBus.RequestCloseMenu(_menuPanel);
	}
}
