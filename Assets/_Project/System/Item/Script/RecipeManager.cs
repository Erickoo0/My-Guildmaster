using System.Collections.Generic;
using UnityEngine;
public class RecipeManager : MonoBehaviour
{

	[Header("Recipes")]
	[SerializeField] private List<ItemDataSo> defaultRecipesList = new List<ItemDataSo>();
	public static RecipeManager Instance { get; private set; }
	public List<ItemDataSo> UnlockedRecipesList { get; private set; } = new List<ItemDataSo>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start()
	{
		foreach (ItemDataSo recipe in defaultRecipesList)
		{
			UnlockRecipe(recipe);
		}
	}

	public void UnlockRecipe(ItemDataSo newRecipe)
	{
		// 1. Check if the newRecipe is already unlocked
		if (UnlockedRecipesList.Contains(newRecipe))
			return;

		// 2. Check if the newRecipe has a crafting recipe
		if (!newRecipe.TryGetProperty<ItemPropertyCraftingRecipe>(out _))
			return; // If it does not, stop here

		UnlockedRecipesList.Add(newRecipe);
		EventBus.RequestRecipeUnlocked(newRecipe);
		Debug.Log($"RecipeManager: Unlocked {newRecipe.ItemName} recipe");
	}
}
