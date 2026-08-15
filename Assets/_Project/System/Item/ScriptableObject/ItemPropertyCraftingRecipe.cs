using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ItemPropertyCraftingRecipe
{

	[field: SerializeField] public List<ResourceRequirement> requiredResources { get; private set; }
	[field: SerializeField] public int CraftingTime { get; private set; } = 5;
	[Serializable]
	public struct ResourceRequirement
	{
		public ItemDataSo requiredItem;
		public int requiredAmount;
	}
}
