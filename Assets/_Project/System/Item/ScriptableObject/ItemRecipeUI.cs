using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemRecipeUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image icon;
	[SerializeField] private TextMeshProUGUI itemName;
	//[SerializeField] private TextMeshProUGUI description;
	[SerializeField] private TextMeshProUGUI requiredResources;
	[SerializeField] private Button craftButton;

	private ItemDataSo _recipeData;
	private ItemPropertyCraftingRecipe _recipeProperty;

	public void Setup(ItemDataSo recipeData)
	{
		_recipeData = recipeData;

		// 1. Check if the ItemDataSo has a crafting recipe property
		if (recipeData.TryGetProperty<ItemPropertyCraftingRecipe>(out _recipeProperty))
		{
			// 2. Set the visuals
			icon.sprite = recipeData.ItemIcon[0]; // Just use the first frame for now
			itemName.text = recipeData.ItemName;
			//description.text = recipeData.ItemDescription;

			// 3. Build the required resources text
			string requirementText = "";
			foreach (ItemPropertyCraftingRecipe.ResourceRequirement requirement in _recipeProperty.RequiredResourcesList)
				requirementText += ($"{requirement.Amount} x {requirement.ItemDataSo.ItemName}\n");
			requiredResources.text = requirementText;

			// 4. Hook the button click
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

		// 3. Make the text red if we don't have enough resources
		requiredResources.color = canCraft ? Color.white : Color.red;
	}

	private void OnCraftButtonClicked() => EventBus.RequestCraftItem(_recipeData);
}
