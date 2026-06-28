using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds all player SkillTreeLedgers.
/// Handles runtime access and save/load integration
/// </summary>
public class SkillTreeLedgerController : MonoBehaviour, ISaveable
{
    public static SkillTreeLedgerController Instance { get; private set; }
    
    [SerializeField] private List<SkillTreeLedger> _skillTreeLedgers = new List<SkillTreeLedger>();
    
    public IReadOnlyList<SkillTreeLedger> SkillTreeLedgers => _skillTreeLedgers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    /// <summary>
    /// Finds the SkillTreeLedger for the given SkillData ID, or creates one if missing.
    /// </summary>
    public SkillTreeLedger GetOrCreateSkillTreeLedger(string skillID)
    {
        if (string.IsNullOrWhiteSpace(skillID)) return null;

        SkillTreeLedger existing = GetSkillTreeLedger(skillID);
        if (existing != null) return existing;

        SkillTreeLedger newLedger = new SkillTreeLedger(skillID);
        _skillTreeLedgers.Add(newLedger);
        return newLedger;
    }
    
    /// <summary>
    /// Finds the SkillTreeLedger for the given SkillData ID without creating one.
    /// </summary>
    public SkillTreeLedger GetSkillTreeLedger(string skillID)
    {
        if (string.IsNullOrWhiteSpace(skillID) || _skillTreeLedgers == null) return null;

        foreach (SkillTreeLedger ledger in _skillTreeLedgers)
            if (ledger != null && ledger.SkillDataID == skillID) return ledger;

        return null;
    }
    
    public int GetAllocatedSkillPoints(string skillID, string skillNodeID)
    {
        SkillTreeLedger ledger = GetSkillTreeLedger(skillID);
        return ledger != null ? ledger.GetAllocatedSkillPoints(skillNodeID) : 0;
    }
    
    public void SetAllocatedPoints(string skillID, string skillNodeID, int skillPoints)
    {
        GetOrCreateSkillTreeLedger(skillID)?.SetAllocatedSkillPoints(skillNodeID, skillPoints);
    }
    
    // ISaveable
    public void PopulateSaveData(SaveData saveData) => saveData.SkillTreeLedgers = _skillTreeLedgers;
    public void LoadFromSaveData(SaveData saveData) => _skillTreeLedgers = saveData.SkillTreeLedgers ?? new List<SkillTreeLedger>();
}
