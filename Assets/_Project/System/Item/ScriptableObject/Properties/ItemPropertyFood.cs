using System;
using UnityEngine;
[Serializable]
public class ItemPropertyFood : ItemPropertyBase
{
	[Tooltip("How much sustenance this provides to a worker")]
	[field: SerializeField] public int SustenanceValue { get; private set; } = 10;
}
