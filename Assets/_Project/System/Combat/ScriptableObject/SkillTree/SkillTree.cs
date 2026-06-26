using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Blueprint for a single Skill's Skill Tree.
/// Binds a SkillData to a SkillTreeLedger.
/// Validates allocation/refund rules by comparing static node rules
/// against a runtime SkillTreeLedger.
/// </summary>
[CreateAssetMenu(fileName = "Skill_Tree_", menuName = "Skills/Skill Tree")]
public class SkillTree : ScriptableObject
{
    [Header("Spell Binding")]
    [Tooltip("The base skill this skill tree modifies.")]
    [SerializeField] private SkillData _skillData;

    [Header("Skill Nodes")]
    [SerializeField] private List<SkillNode> _skillNodes = new List<SkillNode>();

    public SkillData SkillData => _skillData;
    public IReadOnlyList<SkillNode> SkillNodes => _skillNodes;
    
    public SkillNode GetSkillNodeByID(string skillNodeID)
    {
        // 1. If unable to find the ID, return null
        if (string.IsNullOrWhiteSpace(skillNodeID) || _skillNodes == null)
            return null;

        // 2. Loop through all skill nodes in the tree and 
        // find the specific node that matches the given ID
        foreach (SkillNode skillNode in _skillNodes)
        {
            if (skillNode != null && skillNode.ID == skillNodeID)
                return skillNode;
        }

        return null;
    }
    
    public bool ContainsNode(string skillNodeID) => GetSkillNodeByID(skillNodeID) != null;

    /// <summary>
    /// Convenience wrapper for checking if the prerequisites for the given node are met.
    /// Returns true if all prerequisites are met, false otherwise.
    /// </summary>
    public bool CheckPrerequisitesMet(string skillNodeID, SkillTreeLedger skillTreeLedger)
    {
        SkillNode skillNode = GetSkillNodeByID(skillNodeID);
        if (skillNode == null)
            return false;

        return skillNode.CheckPrerequisitesMet(skillTreeLedger);
    }

    /// <summary>
    /// Checks if the given node can be allocated an additional point.
    /// This method only checks rules. TryAllocatePoint performs the actual allocation.
    /// </summary>
    public bool CheckSkillPointAllocation(string skillNodeID, SkillTreeLedger skillTreeLedger)
    {
        // 1. Check if the ledger exists
        if (skillTreeLedger == null)
            return false;

        // 2. Check if the SkillNode exists in the skill tree
        SkillNode skillNode = GetSkillNodeByID(skillNodeID);
        if (skillNode == null)
            return false;

        // 3. Check if at max points allocated
        int skillPointsCurrent = skillTreeLedger.GetAllocatedSkillPoints(skillNodeID);
        if (skillPointsCurrent >= skillNode.SkillPointsMax)
            return false;

        // 4. Check if the skillNode has all prerequisites met
        if (!skillNode.CheckPrerequisitesMet(skillTreeLedger))
            return false;

        // 5. Return true only if all conditions are met
        return true;
    }

    public bool TrySkillPointAllocation(string skillNodeID, SkillTreeLedger skillTreeLedger)
    {
        if (!CheckSkillPointAllocation(skillNodeID, skillTreeLedger))
            return false;

        skillTreeLedger.AddSkillPoint(skillNodeID);
        return true;
    }

    /// <summary>
    /// Checks if the given node can be refunded an additional point.
    /// Blocks refunds if another node in the tree depends on this node.
    /// </summary>
    public bool CheckSkillPointRefund(string skillNodeID, SkillTreeLedger skillTreeLedger)
    {
        // Safety Check
        if (skillTreeLedger == null)
            return false;

        SkillNode skillNode = GetSkillNodeByID(skillNodeID);
        if (skillNode == null)
            return false;

        // 1. If the node has 0 points allocated, return false
        if (skillTreeLedger.GetAllocatedSkillPoints(skillNodeID) <= 0)
            return false;

        // 2. Loop through all SkillNodes in the SkillTree
        // If another SkillNode depends on this SkillNode, return false
        foreach (SkillNode otherSkillNode in _skillNodes)
        {
            // Skip null and this node
            if (otherSkillNode == null || otherSkillNode.ID == skillNodeID)
                continue;

            // Skip nodes with 0 points allocated
            if (skillTreeLedger.GetAllocatedSkillPoints(otherSkillNode.ID) <= 0)
                continue;

            // If the other SkillNode requires this SkillNode, return false
            if (otherSkillNode.HasPrerequisite(skillNodeID))
                return false;
        }

        // 3. Otherwise, return true
        return true;
    }

    public bool TrySkillPointRefund(string skillNodeID, SkillTreeLedger skillTreeLedger)
    {
        if (!CheckSkillPointRefund(skillNodeID, skillTreeLedger))
            return false;

        skillTreeLedger.RemoveSkillPoint(skillNodeID);
        return true;
    }

    private void OnValidate()
    {
        ValidateNodeIDs();
    }

    /// <summary>
    /// Editor safety check for missing or duplicate SkillNode IDs
    /// </summary>
    private void ValidateNodeIDs()
    {
        if (_skillNodes == null)
            return;

        HashSet<string> seenIDs = new HashSet<string>();

        foreach (SkillNode node in _skillNodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.ID))
            {
                Debug.LogWarning($"{name}: A skill node has an empty ID.", this);
                continue;
            }

            if (!seenIDs.Add(node.ID))
                Debug.LogWarning($"{name}: Duplicate skill node ID found: {node.ID}", this);
        }
    }
}