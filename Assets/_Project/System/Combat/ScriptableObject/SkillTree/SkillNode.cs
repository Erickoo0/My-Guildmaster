using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A node in the SpellSkillTree.
/// Contains Node MetaData, _prerequisites, and modifiers.
/// </summary>
[System.Serializable]
public class SkillNode
{
    [Header("Identity")]
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private Sprite _icon;
    [SerializeField, TextArea] private string _description;

    [Header("UI Layout")]
    [SerializeField] private Vector2 _uiPosition;

    [Header("Point Rules")]
    [Min(1)]
    [SerializeField] private int _skillPointsMax = 1;

    [Header("Prerequisites")]
    [SerializeField] private List<SkillNodePrerequisite> _prerequisites = new List<SkillNodePrerequisite>();

    [Header("Modifiers")]
    [Tooltip("Modifiers applied once for each allocated point in this node.")]
    [SerializeField] private List<ModifierSkillBase> _modifiers = new List<ModifierSkillBase>();

    public string ID => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public Vector2 UIPosition => _uiPosition;
    public int SkillPointsMax => _skillPointsMax;
    public IReadOnlyList<SkillNodePrerequisite> Prerequisites => _prerequisites;
    public IReadOnlyList<ModifierSkillBase> Modifiers => _modifiers;
    
    public bool CheckPrerequisitesMet(SkillTreeLedger ledger)
    {
        // Safety Check
        if (ledger == null) 
            return false;

        // 1. If no prerequisites, then true
        if (_prerequisites == null || _prerequisites.Count == 0) 
            return true;

        // 2. Loop through each prerequisite
        foreach (SkillNodePrerequisite prerequisite in _prerequisites)
        {
            if (prerequisite == null) 
                continue;

            // If any prerequisite is not met, return false
            if (!prerequisite.CheckRequiredSkillNodeIsMet(ledger)) 
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if this node has a prerequisite with the given _skillNodeID.
    /// Used for skill point refund validation
    /// </summary>
    public bool HasPrerequisite(string prerequisiteNodeID)
    {
        if (string.IsNullOrWhiteSpace(prerequisiteNodeID)) 
            return false;

        if (_prerequisites == null) 
            return false;

        foreach (SkillNodePrerequisite prerequisite in _prerequisites)
        {
            if (prerequisite != null && prerequisite.RequiredSkillNodeID == prerequisiteNodeID)
                return true;
        }

        return false;
    }
}