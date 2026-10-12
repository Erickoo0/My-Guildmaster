using System;
using UnityEngine;
/// <summary>
/// Research reward that unlocks an alchemy recipe.
/// </summary>
[Serializable]
public class ResearchRewardAlchemyRecipe : ResearchReward
{
	[SerializeField] private ItemDataSo _recipe;

	public override void ApplyReward()
	{
		if (_recipe == null)
			return;

		EventBus.RequestAlchemyRecipeUnlocked(_recipe);
	}
}
