using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles unlocking of Alchemy Recipes.
/// </summary>
public class AlchemyRecipeManager : MonoBehaviour
{

	[Header("Recipes")]
	[SerializeField] private List<ItemDataSo> defaultRecipesList = new List<ItemDataSo>();
	public static AlchemyRecipeManager Instance { get; private set; }
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

		// 2. Check if the new recipe has a alchemy recipe component
		if (!newRecipe.TryGetProperty<ItemPropertyAlchemyRecipe>(out _))
			return;

		UnlockedRecipesList.Add(newRecipe);
		EventBus.RequestAlchemyRecipeUnlocked(newRecipe);
		Debug.Log($"AlchemyRecipeManager: Unlocked {newRecipe.ItemName} recipe");
	}
}
