using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles displaying an individual AlchemyRecipeSlotUI
/// </summary>
public class AlchemyRecipeSlotUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image _itemIcon;
	[SerializeField] private Button _recipeButton; // The button this script is attached to

	public ItemDataSo RecipeData { get; private set; }

	public event Action<AlchemyRecipeSlotUI, ItemDataSo> OnRecipeSelected;

	public void Setup(ItemDataSo recipeData)
	{
		RecipeData = recipeData;

		// 1. Check if the ItemDataSo has an alchemy recipe
		if (recipeData.TryGetProperty<ItemPropertyAlchemyRecipe>(out _))
		{
			// 2. Set the visuals
			_itemIcon.sprite = recipeData.ItemIcon[0];

			// 3. Hook the button click
			_recipeButton.onClick.RemoveAllListeners();
			_recipeButton.onClick.AddListener(() => OnRecipeSelected?.Invoke(this, RecipeData));
		}
	}
}
