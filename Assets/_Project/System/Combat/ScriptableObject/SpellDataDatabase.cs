using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SpellDataDatabase", menuName = "SpellData/SpellDataDatabase", order = 1)]
public class SpellDataDatabase : ScriptableObject
{
    [SerializeField] private List<SpellData> allSpells;
    
    private Dictionary<string, SpellData> _spellDictionary;

    private void SetupDictionary()
    {
        if (_spellDictionary != null) return;

        _spellDictionary = new Dictionary<string, SpellData>();
        foreach (var spell in allSpells)
        {
            if (spell == null)
            {
                Debug.LogWarning("[SpellDatabase] SpellData is null!");
                continue;
            }

            // Pairs spellID to spellDataSource, returns warning if spellID is a duplicate
            if (!_spellDictionary.TryAdd(spell.spellID, spell))
            {
                Debug.LogWarning($"[SpellDatabase] Duplicate Spell ID found: {spell.spellID}. IDs must be unique!");
            }
        }
    }

    public T GetSpell<T>(string spellID) where T : SpellData
    {
        SetupDictionary(); // Ensure dictionary is built

        if (_spellDictionary.TryGetValue(spellID, out SpellData spellData) && spellData is T typedData)
            return typedData;
        
        Debug.LogError($"Spell Database: Could not find spell with ID {spellID}");
        return null;
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Refresh Database")]
    public void AutoPopulateDatabase()
    {
        allSpells.Clear();
        
        // Find every asset of type SpellData
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(SpellData)}");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpellData spell = AssetDatabase.LoadAssetAtPath<SpellData>(path);
            
            if (spell != null && !allSpells.Contains(spell))
            {
                allSpells.Add(spell);
            }
        }

        // Tell Unity we changed this file so it remembers to save the new list
        EditorUtility.SetDirty(this);
        Debug.Log($"[SpellDatabase] Successfully auto-populated {allSpells.Count} spells.");
    }
    #endif
}
