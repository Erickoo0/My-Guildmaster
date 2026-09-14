using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles displaying an individual ItemRecipeSlotUI
/// </summary>
public class ItemRecipeSlotUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image _itemIcon;
	[SerializeField] private Button _recipeButton; // The button this script is attached to


	public ItemDataSo RecipeData { get; private set; }


	public event Action<ItemRecipeSlotUI, ItemDataSo> OnRecipeSelected;

	public void Setup(ItemDataSo recipeData)
	{
		RecipeData = recipeData;

		// 1. Check if the ItemDataSo has a crafting recipe property
		if (recipeData.TryGetProperty<ItemPropertyCraftingRecipe>(out _))
		{
			// 2. Set the visuals
			_itemIcon.sprite = recipeData.ItemIcon[0]; // Just use the first frame for now

			// 3. Hook the button click
			_recipeButton.onClick.RemoveAllListeners();
			_recipeButton.onClick.AddListener(() => OnRecipeSelected?.Invoke(this, RecipeData));
		}
	}
}
