using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles unlocking of Crafting Recipes
/// </summary>
public class CraftingRecipeManager : MonoBehaviour
{

	[Header("Recipes")]
	[SerializeField] private List<ItemDataSo> defaultRecipesList = new List<ItemDataSo>();
	public static CraftingRecipeManager Instance { get; private set; }
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
		// Unlock all default recipes
		foreach (ItemDataSo recipe in defaultRecipesList)
			UnlockRecipe(recipe);
	}

	public void UnlockRecipe(ItemDataSo newRecipe)
	{
		// 1. Check if we already have this recipe unlocked
		if (UnlockedRecipesList.Contains(newRecipe))
			return;

		// 2. Check if the newRecipe has a crafting recipe component
		if (!newRecipe.TryGetProperty<ItemPropertyCraftingRecipe>(out _))
			return; // If it does not, stop here

		UnlockedRecipesList.Add(newRecipe);
		EventBus.RequestRecipeUnlocked(newRecipe);
		Debug.Log($"CraftingRecipeManager: Unlocked {newRecipe.ItemName} recipe");
	}
}
