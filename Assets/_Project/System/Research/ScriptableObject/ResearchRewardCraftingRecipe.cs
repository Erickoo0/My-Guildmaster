using System;
using UnityEngine;
/// <summary>
/// Research reward that unlocks a crafting recipe.
/// </summary>
[Serializable]
public class ResearchRewardCraftingRecipe : ResearchReward
{
	[SerializeField] private ItemDataSo _recipe;

	public override void ApplyReward()
	{
		if (_recipe == null)
			return;

		EventBus.RequestCraftingRecipeUnlocked(_recipe);
	}
}
