using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemCategory{Tool, Weapon, Armor, Ring, Amulet, Potion, Food, Seed, Decor, Resource, Quest}
public enum ItemRarity{Common, Uncommon, Rare, Epic, Mythic}

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
    public bool IsAnimated => ItemIcon != null && ItemIcon.Length > 1;

    [field: Header("Economy & Rules")]
    [field: SerializeField] public int ItemValue { get; private set; }
    [field: SerializeField] public bool IsSellable { get; private set; } = true;
    [field: SerializeField] public bool IsStackable { get; private set; } = true;
    [field: SerializeField] public int MaxStackSize { get; private set; } = 99;

    [field: Header("Usage & EffectsList")] 
    [field: SerializeField] public bool IsUsable { get; private set; }
    [SerializeReference, SubclassSelector] public List<Effect> effects = new List<Effect>();

    public bool Use(ItemInstance itemInstance, GameObject user, GameObject target = null, Vector3 targetPosition = default)
    {
        // If there is no usable effects, do nothing
        if (!IsUsable || effects == null || effects.Count == 0) return false;

        bool anyEffectSucceeded = false;
        
        // 1. Construct the effectPayload
        EffectPayload effectPayload = new EffectPayload(user, target, targetPosition);
        
        // 2. Loop through every effect attached to this item
        foreach (Effect effect in effects)
        {
            // If any effect succeeds, return true
            if (effect.Execute(effectPayload))
                anyEffectSucceeded = true;
        }
        
        return anyEffectSucceeded;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically sync ID with File Name
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (ItemID != fileName)
            {
                ItemID = fileName;
                // Mark as dirty to ensure the change is saved
                EditorUtility.SetDirty(this);
            }
        }

        // Logic safety: Ensure stack size is at least 1 if stackable
        if (IsStackable && MaxStackSize < 1)
        {
            MaxStackSize = 1;
        }
    }
#endif
}