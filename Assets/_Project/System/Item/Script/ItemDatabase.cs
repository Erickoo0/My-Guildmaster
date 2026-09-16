using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// SO that holds a dictionary of all ItemDataSO
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Database")]
public class ItemDatabase : ScriptableObject
{
	[Tooltip("Auto-populated list of all items. Do not manage manually.")]
	public List<ItemDataSo> allItems;

	// Key: Items String ID
	// Value: Items ItemData
	private Dictionary<string, ItemDataSo> _itemDictionary;

	public void Initialize()
	{
		// Create a fresh dictionary
		_itemDictionary = new Dictionary<string, ItemDataSo>();

		// Loop through every item in the database
		foreach (ItemDataSo itemData in allItems)
		{
			if (itemData == null)
			{
				Debug.LogWarning("[ItemDatabase] ItemData is null!");
				continue;
			}
			// Pairs itemID to itemData, returns warning if itemID has already been previously used
			if (!_itemDictionary.TryAdd(itemData.ItemID, itemData))
			{
				Debug.LogWarning($"[ItemDatabase] Duplicate Item ID found: {itemData.ItemID}. IDs must be unique!");
			}
		}
	}

	public ItemDataSo GetItem(string itemID)
	{
		// Safety Check: If the dictionary hasn't been built yet, build it now
		if (_itemDictionary == null) Initialize();

		// If the ID exists, returns the Item, else return null
		// ReSharper disable once PossibleNullReferenceException
		return _itemDictionary.GetValueOrDefault(itemID);
	}

    #if UNITY_EDITOR
	[ContextMenu("Refresh Database")]
	public void AutoPopulateDatabase()
	{
		allItems.Clear();

		// Find every asset in the project of type ItemDataSo
		string[] guids = AssetDatabase.FindAssets($"t:{nameof(ItemDataSo)}");

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			ItemDataSo item = AssetDatabase.LoadAssetAtPath<ItemDataSo>(path);

			if (item != null && !allItems.Contains(item))
			{
				allItems.Add(item);
			}
		}

		// Tell Unity we changed this file so it remembers to save the new list
		EditorUtility.SetDirty(this);
		Debug.Log($"[ItemDatabase] Successfully auto-populated {allItems.Count} items.");
	}
    #endif
}
