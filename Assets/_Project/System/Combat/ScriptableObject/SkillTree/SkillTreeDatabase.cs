using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SkillTreeDatabase", menuName = "Skills/Skill Tree Database")]
public class SkillTreeDatabase : ScriptableObject
{
    [Header("Skill Trees")]
    [SerializeField] private List<SkillTree> _skillTreesList = new List<SkillTree>();
    private Dictionary<string, SkillTree> _skillTreeDictionary;

    private void BuildDictionary()
    {
        if (_skillTreeDictionary != null) return;
        
        _skillTreeDictionary = new Dictionary<string, SkillTree>();
        foreach (SkillTree skillTree in _skillTreesList)
        {
            if (skillTree == null || skillTree.SkillData == null)
            {
                Debug.LogWarning($"{name}: SkillTree is null or SkillData is null!");
                continue;
            }
            
            if (!_skillTreeDictionary.TryAdd(skillTree.SkillData.ID, skillTree))
                Debug.LogWarning($"{name}: Duplicate SkillTree for skill ID '{skillTree.SkillData.ID}'.");
        }
    }
    
    public SkillTree GetSkillTreeByID(string skillDataID)
    {
        // 1. Ensure the dictionary is built
        BuildDictionary();
        
        if (string.IsNullOrWhiteSpace(skillDataID)) return null;

        // 2. Look in the dictionary for the skill tree that has the given skill data ID
        _skillTreeDictionary.TryGetValue(skillDataID, out SkillTree tree);
        return tree; // null if no tree assigned
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Refresh Database")]
    public void AutoPopulateDatabase()
    {
        _skillTreesList.Clear();

        string[] guids = AssetDatabase.FindAssets($"t:{nameof(SkillTree)}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SkillTree tree = AssetDatabase.LoadAssetAtPath<SkillTree>(path);
            if (tree != null && !_skillTreesList.Contains(tree))
                _skillTreesList.Add(tree);
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[SkillTreeDatabase] Auto-populated {_skillTreesList.Count} trees.");
    }
    #endif
}

