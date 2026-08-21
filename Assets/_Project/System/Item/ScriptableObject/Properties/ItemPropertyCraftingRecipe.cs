using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ItemPropertyCraftingRecipe : ItemPropertyBase
{

	[field: SerializeField] public List<ResourceRequirement> RequiredResourcesList { get; private set; }
	[field: SerializeField] public int CraftingTime { get; private set; } = 5;
	[Serializable]
	public struct ResourceRequirement
	{
		public ItemDataSo ItemDataSo;
		public int Amount;
	}
}
