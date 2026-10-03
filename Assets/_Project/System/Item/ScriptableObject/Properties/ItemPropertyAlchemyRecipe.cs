using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ItemPropertyAlchemyRecipe : ItemPropertyBase
{
	[field: SerializeField] public List<ResourceRequirement> RequiredResourcesList { get; private set; }
	[field: SerializeField] public int BrewingTime { get; private set; }
	[Serializable]
	public struct ResourceRequirement
	{
		public ItemDataSo ItemDataSo;
		public int Amount;
	}
}
