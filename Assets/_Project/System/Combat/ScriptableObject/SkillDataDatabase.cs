using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SkillDataDatabase", menuName = "Skills/SkillData Database", order = 1)]
public class SkillDataDatabase : ScriptableObject
{
    [Header("Skill Data")]
    [SerializeField] private List<SkillData> _skillList;
    private Dictionary<string, SkillData> _skillDictionary;

    private void BuildDictionary()
    {
        if (_skillDictionary != null) return;

        _skillDictionary = new Dictionary<string, SkillData>();
        foreach (SkillData spell in _skillList)
        {
            if (spell == null)
            {
                Debug.LogWarning($"{name}: SkillData is null!");
                continue;
            }

            // Pairs ID to SkillDataSource, returns warning if ID is a duplicate
            if (!_skillDictionary.TryAdd(spell.ID, spell))
                Debug.LogWarning($"[SpellDatabase] Duplicate Spell ID found: {spell.ID}. IDs must be unique!");
            
        }
    }

    public T GetSkillDataByID<T>(string skillID) where T : SkillData
    {
        BuildDictionary(); // Ensure dictionary is built

        if (_skillDictionary.TryGetValue(skillID, out SkillData skillData) && skillData is T typedData)
            return typedData;
        
        Debug.LogError($"Skill Database: Could not find skill with ID {skillID}");
        return null;
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Refresh Database")]
    public void AutoPopulateDatabase()
    {
        _skillList.Clear();
        
        // Find every asset of type SkillData
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(SkillData)}");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>(path);
            
            if (skill != null && !_skillList.Contains(skill))
            {
                _skillList.Add(skill);
            }
        }

        // Tell Unity we changed this file so it remembers to save the new list
        EditorUtility.SetDirty(this);
        Debug.Log($"[SpellDatabase] Successfully auto-populated {_skillList.Count} spells.");
    }
    #endif
}
