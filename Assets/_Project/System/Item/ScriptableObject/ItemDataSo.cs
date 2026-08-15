using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemCategory { Tool, Weapon, Armor, Ring, Amulet, Potion, Food, Seed, Decor, Resource, Quest }

public enum ItemRarity { Common, Uncommon, Rare, Epic, Mythic }

/// <summary>
/// Blueprint data for an item
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class ItemDataSo : ScriptableObject
{
	[field: Header("Identity")]
	[field: SerializeField] public string ItemID { get; private set; }
	[field: SerializeField] public string ItemName { get; private set; }
	[field: SerializeField, TextArea] public string ItemDescription { get; private set; }
	[field: SerializeField] public ItemCategory Category { get; private set; }
	[field: SerializeField] public ItemRarity Rarity { get; private set; } = ItemRarity.Common;

	[field: Header("Visuals")]
	[field: SerializeField] public Sprite[] ItemIcon { get; private set; }
	[Tooltip("Leave blank for default item object")]
	[field: SerializeField] public GameObject ItemObject { get; private set; }

	[field: Header("Economy & Rules")]
	[field: SerializeField] public int ItemValue { get; private set; }
	[field: SerializeField] public bool IsSellable { get; private set; } = true;
	[field: SerializeField] public bool IsStackable { get; private set; } = true;
	[field: SerializeField] public int MaxStackSize { get; private set; } = 99;

	[field: Header("Properties")]
	[SerializeReference, SubclassSelector] public List<ItemPropertyBase> ItemProperties = new List<ItemPropertyBase>();

	[field: Header("Usage & Effects")]
	[field: SerializeField] public bool IsUsable { get; private set; }
	[SerializeReference, SubclassSelector] public List<Effect> effects = new List<Effect>();

	public bool IsAnimated => ItemIcon != null && ItemIcon.Length > 1;

#if UNITY_EDITOR
	private void OnValidate()
	{
		string path = AssetDatabase.GetAssetPath(this);
		if (!string.IsNullOrEmpty(path))
		{
			string fileName = Path.GetFileNameWithoutExtension(path);
			if (ItemID != fileName)
			{
				ItemID = fileName;
				EditorUtility.SetDirty(this);
			}
		}

		if (IsStackable && MaxStackSize < 1)
		{
			MaxStackSize = 1;
		}
	}
#endif

	/// <summary>
	/// Attempt to execute all effects associated with this item
	/// </summary>
	public bool Use(ItemInstance itemInstance, GameObject user, GameObject target = null, Vector3 targetPosition = default)
	{
		if (!IsUsable || effects == null || effects.Count == 0) return false;

		// 1. Construct the Payload 
		EffectPayload effectPayload = new EffectPayload(user)
		{
			Target = target != null ? target : user,
			TargetPosition = targetPosition
		};

		// 2. Execute Effects
		bool anyEffectSucceeded = false;
		foreach (Effect effect in effects)
		{
			if (effect.Execute(effectPayload))
				anyEffectSucceeded = true;
		}

		return anyEffectSucceeded;
	}

	/// <summary>
	/// Checks if an item has a specific property.
	/// </summary>
	public bool TryGetProperty<T>(out T property) where T : ItemPropertyBase
	{
		// 1. Loop through all properties
		foreach (ItemPropertyBase prop in ItemProperties)
		{
			// 2. If the property is of the correct type, return it
			if (prop is T match)
			{
				property = match;
				return true;
			}
		}

		property = null;
		return false;
	}
}
