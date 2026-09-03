using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles displaying na individual ItemRecipeUI
/// </summary>
public class ItemRecipeUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image itemIcon;
	[SerializeField] private TextMeshProUGUI itemName;
	[SerializeField] private Button craftButton;

	private ItemDataSo _recipeData;
	private ItemPropertyCraftingRecipe _recipeProperty;

	public string TooltipTitle { get; set; }
	public string TooltipDescription { get; set; }

	public void Setup(ItemDataSo recipeData)
	{
		_recipeData = recipeData;

		// 1. Check if the ItemDataSo has a crafting recipe property
		if (recipeData.TryGetProperty<ItemPropertyCraftingRecipe>(out _recipeProperty))
		{
			// 2. Set the visuals
			itemIcon.sprite = recipeData.ItemIcon[0]; // Just use the first frame for now
			itemName.text = recipeData.ItemName;

			// 3. Hook the button click
			craftButton.onClick.RemoveAllListeners();
			craftButton.onClick.AddListener(OnCraftButtonClicked);
		}
	}

	public void RefreshState()
	{
		// Safety Check
		if (_recipeData == null || _recipeProperty == null)
			return;

		// 1. Ask the CraftingManager if we have enough resources in our inventory
		bool canCraft = CraftingManager.Instance.HasResources(_recipeProperty);

		// 2. Turn the button off if we dont have enough resources
		craftButton.interactable = canCraft;

		// 3. Build the description
		TooltipTitle = _recipeData.ItemName;
		string resourceText = _recipeData.ItemDescription + "\n\n<b>Required Materials:</b>\n";

		// 4. Loop through the required resources and count how many we have in our inventory
		foreach (ItemPropertyCraftingRecipe.ResourceRequirement requirement in _recipeProperty.RequiredResourcesList)
		{
			int totalFound = 0;

			for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
				totalFound += ItemStoragePlayer.Instance.GetItem(i)?.stackSize ?? 0;

			// 5. Change resource color based on if we have enough
			string colorTag = totalFound >= requirement.Amount ? "<color=#ffffff>" : "<color=#ff4444>";

			// 6. Format the required resource
			resourceText += $"{colorTag}{requirement.Amount} x {requirement.ItemDataSo.ItemName} ({totalFound}/{requirement.Amount})</color>\n";
		}

		TooltipDescription = resourceText;
	}

	private void OnCraftButtonClicked() => EventBus.RequestCraftItem(_recipeData);
}
