using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime/Save-state allocation ledger for one specific skill tree.
/// Stores which spell it belongs to,
/// which SkillNodes have been allocated skill points, and how many points.
/// </summary>
[System.Serializable]
public class SkillTreeLedger
{
    [SerializeField] private string _spellDataID;
    [SerializeField] private List<SkillNodeAllocation> _allocations;

    public string SpellDataID => _spellDataID;
    public IReadOnlyList<SkillNodeAllocation> Allocations => _allocations;

    // Constructor
    public SkillTreeLedger(string spellDataID)
    {
        _spellDataID = spellDataID;
        _allocations = new List<SkillNodeAllocation>();
    }

    /// <summary>
    /// Returns the number of skill points allocated to a SkillNode.
    /// Missing allocation entries are treated as 0 points.
    /// </summary>
    public int GetAllocatedSkillPoints(string skillNodeID)
    {
        SkillNodeAllocation allocation = GetSkillNodeAllocation(skillNodeID);
        return allocation != null ? allocation.AllocatedSkillPoints : 0;
    }

    /// <summary>
    /// Sets the number of skill points allocated to a SkillNode.
    /// A value of 0 or less removes the allocation entry entirely, keeping
    /// the serialized list compact and preventing saved SkillTreeLedgers from
    /// filling with 0 point records.
    /// </summary>
    public void SetAllocatedSkillPoints(string skillNodeID, int value)
    {
        // Safety CHeck
        if (string.IsNullOrWhiteSpace(skillNodeID))
            return;

        // 1. Clamp the value to 0 if its negative
        value = Mathf.Max(0, value);

        // 2. Get the allocation
        SkillNodeAllocation allocation = GetSkillNodeAllocation(skillNodeID);

        // 3. Remove entries with 0 or less allocation
        if (value <= 0)
        {
            if (allocation != null)
                _allocations.Remove(allocation);

            return;
        }

        // 4. If allocation doesn't exist, create it and add to list
        if (allocation == null)
        {
            allocation = new SkillNodeAllocation(skillNodeID, value);
            _allocations.Add(allocation);
        }
        else // 5. else, simply set its points
        {
            allocation.SetAllocatedSkillPoints(value);
        }
    }

    /// <summary>
    /// Increments a SkillNode's allocated points by 1.
    /// Validation such as prerequisites and max point cap should happen
    /// within the SkillTree before this method is called.
    /// </summary>
    public void AddSkillPoint(string skillNodeID)
    {
        int currentPoints = GetAllocatedSkillPoints(skillNodeID);
        SetAllocatedSkillPoints(skillNodeID, currentPoints + 1);
    }

    /// <summary>
    /// Decrements a SkillNode's allocated points by 1.
    /// Refund validation should happen within the SkillTree before this method is called.
    /// </summary>
    /// <param name="skillNodeID"></param>
    public void RemoveSkillPoint(string skillNodeID)
    {
        int currentPoints = GetAllocatedSkillPoints(skillNodeID);
        SetAllocatedSkillPoints(skillNodeID, currentPoints - 1);
    }
    
    public bool HasAnyPoints(string skillNodeID) => GetAllocatedSkillPoints(skillNodeID) > 0;

    public int GetTotalAllocatedSkillPoints()
    {
        int total = 0;

        if (_allocations == null)
            return total;

        foreach (SkillNodeAllocation allocation in _allocations)
        {
            if (allocation != null)
                total += allocation.AllocatedSkillPoints;
        }

        return total;
    }

    /// <summary>
    /// Internal lookup helper method for the serialized list of allocations
    /// </summary>
    private SkillNodeAllocation GetSkillNodeAllocation(string skillNodeID)
    {
        // Safety Check
        if (string.IsNullOrWhiteSpace(skillNodeID) || _allocations == null)
            return null;

        // Loop through the list of allocations and find the matching skillNodeID
        foreach (SkillNodeAllocation allocation in _allocations)
        {
            if (allocation != null && allocation.SkillNodeID == skillNodeID)
                return allocation;
        }

        return null;
    }
}

/// <summary>
/// Serializable record for one allocated SkillNode.
/// Links to the SkillNode.ID in the SkillTree.
/// </summary>
[System.Serializable]
public class SkillNodeAllocation
{
    [SerializeField] private string _skillNodeID;
    [SerializeField] private int _allocatedSkillPoints;

    public string SkillNodeID => _skillNodeID;
    public int AllocatedSkillPoints => _allocatedSkillPoints;

    public SkillNodeAllocation(string skillNodeID, int allocatedPoints)
    {
        _skillNodeID = skillNodeID;
        _allocatedSkillPoints = Mathf.Max(0, allocatedPoints);
    }

    public void SetAllocatedSkillPoints(int value)
    {
        _allocatedSkillPoints = Mathf.Max(0, value);
    }
}