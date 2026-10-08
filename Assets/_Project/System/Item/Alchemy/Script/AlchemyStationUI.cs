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
	[SerializeField] private Transform[] _resourceSlotContainers;
	[SerializeField] private ItemSlotUI _itemSlotUI;

	[Header("Item Detail")]
	[SerializeField] private TextMeshProUGUI _itemName;
	[SerializeField] private TextMeshProUGUI _itemDescription;
	[SerializeField] private Image _itemIcon;

	[Header("Brewing")]
	[SerializeField] private GameObject _brewingProgressBar;
	[SerializeField] private Image _brewingProgressBarFill;
	[SerializeField] private Button _brewButton;

	private List<GameObject> _activeResourceSlotsList = new List<GameObject>();
	private AlchemyStationManager _currentAlchemyStation;
	private List<AlchemyRecipeSlotUI> _recipeSlotsList = new List<AlchemyRecipeSlotUI>();

	private ItemDataSo _selectedRecipe;

	private void Start()
	{
		EventBus.OnAlchemyStationOpened += HandleStationOpened;
		EventBus.OnAlchemyRecipeUnlocked += CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryUpdated;
		_brewButton.onClick.AddListener(OnBrewButtonClicked);

		// 1. Create all initial unlocked recipeSlotUI
		foreach (ItemDataSo recipe in AlchemyRecipeManager.Instance.UnlockedRecipesList)
			CreateRecipeSlotUI(recipe);

		// 2. Clear item details
		ClearItemDetails();
	}

	private void Update()
	{
		// 1. Only run if menu is active
		if (!_menuPanel.activeSelf || _currentAlchemyStation == null)
			return;

		// 2. Only run if alchemy station is brewing
		if (_currentAlchemyStation.IsBrewing)
		{
			_brewingProgressBarFill.fillAmount = _currentAlchemyStation.BrewProgress;
			_brewButton.interactable = false;
		}
	}

	private void OnDestroy()
	{
		EventBus.OnAlchemyStationOpened -= HandleStationOpened;
		EventBus.OnAlchemyRecipeUnlocked -= CreateRecipeSlotUI;
		ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryUpdated;

		if (_currentAlchemyStation != null)
			_currentAlchemyStation.OnSlotUpdated -= HandleStationUpdated;
	}


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

	private void RefreshItemDetails()
	{
		_itemIcon.color = Color.white;

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

		// 3. Spawn new resource slot prefabs into their specific containers and pass them the inventory counts
		int containerIndex = 0;
		foreach (ItemPropertyAlchemyRecipe.ResourceRequirement requirement in recipe.RequiredResourcesList)
		{
			// Safety Check
			if (containerIndex >= _resourceSlotContainers.Length)
			{
				Debug.LogWarning("AlchemyStationUI: Not enough resource slot containers for all required resources.");
				break;
			}

			int totalFound = 0;

			// 4. Get the total ingredients found in inventory
			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
			{
				ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);
				if (item != null && item.DataSo == requirement.ItemDataSo)
					totalFound += item.stackSize;
			}

			// 5. Create the prefab and add it to the list
			Transform targetContainer = _resourceSlotContainers[containerIndex];
			GameObject resourceSlotUI = Instantiate(_resourceSlotPrefab, targetContainer);
			_activeResourceSlotsList.Add(resourceSlotUI);
			containerIndex++;

			// 6. Pass the data to the prefab
			if (resourceSlotUI.TryGetComponent(out AlchemyResourceSlotUI resourceSlot))
				resourceSlot.Setup(requirement.ItemDataSo, requirement.Amount, totalFound);
		}

		// 7. Check if the station has resources and the output slot is empty
		bool hasResources = _currentAlchemyStation.HasResources(recipe);
		bool itemSlotEmpty = _currentAlchemyStation.GetItem(0) == null;
		bool isBrewing = _currentAlchemyStation.IsBrewing;

		_brewButton.interactable = hasResources && itemSlotEmpty && !isBrewing;

		// 8. Reset the progress bar
		_brewingProgressBarFill.fillAmount = 0f;
	}

	private void ClearItemDetails()
	{
		_selectedRecipe = null;
		_itemIcon.color = Color.clear;
		_itemName.text = "";
		_itemDescription.text = "";
		_brewButton.interactable = false;
	}

	#region Event Handlers

	private void HandleInventoryUpdated(int _) => RefreshItemDetails();

	private void HandleStationUpdated(int _)
	{
		_itemSlotUI.RefreshSlotUI();
		RefreshItemDetails();
	}

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

	private void HandleStationOpened(AlchemyStationManager station)
	{
		// If the menu is already open AND they clicked the exact same station, close it!
		if (_menuPanel.activeSelf && _currentAlchemyStation == station)
		{
			EventBus.RequestCloseMenu(_menuPanel);
			_currentAlchemyStation.OnSlotUpdated -= HandleStationUpdated;
			_currentAlchemyStation = null;
			ClearItemDetails();
			return;
		}

		// 1. Unsubscribe from the previous station's slot updates
		if (_currentAlchemyStation != null)
			_currentAlchemyStation.OnSlotUpdated -= HandleStationUpdated;

		// 2. Subscribe to the new station's slot updates
		_currentAlchemyStation = station;
		_currentAlchemyStation.OnSlotUpdated += HandleStationUpdated;

		// 3. Bind the station to the UI
		_itemSlotUI.Setup(_currentAlchemyStation, 0);

		// 4. Open the menu
		if (!_menuPanel.activeSelf)
			EventBus.RequestOpenMenu(_menuPanel);

		RefreshItemDetails();
	}

	private void OnBrewButtonClicked()
	{
		if (_selectedRecipe != null)
			_currentAlchemyStation.TryBrew(_selectedRecipe);
	}

	#endregion
}
