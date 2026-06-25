using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player container for holding all individual SkillTreeLedgers.
/// </summary>
[System.Serializable]
public class PlayerSkillTreeLedger
{
	[Header("Skill Tree Ledgers")]
	[SerializeField] private List<SkillTreeLedger> _spellLedgers = new List<SkillTreeLedger>();

	public IReadOnlyList<SkillTreeLedger> SpellLedgers => _spellLedgers;

	/// <summary>
	/// Finds the SkillTreeLedger for the given SpellData ID, or creates a new one if it doesn't exist.
	/// This is the main entry point for allocating/refunding skillPoints'
	/// </summary>
	public SkillTreeLedger GetOrCreateSkillTreeLedger(string spellID)
	{
		// Safety Check
		if (string.IsNullOrWhiteSpace(spellID))
			return null;

		// 1. Check for existing SkillTreeLedger
		SkillTreeLedger existingSkillTreeLedger = GetSkillTreeLedger(spellID);
		if (existingSkillTreeLedger != null)
			return existingSkillTreeLedger;

		// 2. Else, create a new SkillTreeLedger and add it to the list of SkillTreeLedgers
		SkillTreeLedger newSkillTreeLedger = new SkillTreeLedger(spellID);
		_spellLedgers.Add(newSkillTreeLedger);

		return newSkillTreeLedger;
	}

	/// <summary>
	/// Checks if the given SpellData ID has a SkillTreeLedger, without creating one.
	/// Useful for if you only want to check.
	/// </summary>
	public SkillTreeLedger GetSkillTreeLedger(string spellID)
	{
		// Safety Check
		if (string.IsNullOrWhiteSpace(spellID) || _spellLedgers == null)
			return null;

		// 1. Loop through each SkillTreeLedger and check if it matches the given SpellData ID
		foreach (SkillTreeLedger ledger in _spellLedgers)
		{
			if (ledger != null && ledger.SpellDataID == spellID)
				return ledger;
		}

		// 2. If no match is found, return null
		return null;
	}

	/// <summary>
	/// Read Helper to get the allocated points for a given SkillNode ID within a given SpellData/SkillTreeLedger.
	/// </summary>
	public int GetAllocatedSkillPoints(string spellID, string skillNodeID)
	{
		// Get the SkillTreeLedger for the given SpellData ID
		SkillTreeLedger skillTreeLedger = GetSkillTreeLedger(spellID);
		
		// Return the allocated points for the given SkillNode ID, or 0 if the ledger is null
		return skillTreeLedger != null ? skillTreeLedger.GetAllocatedSkillPoints(skillNodeID) : 0;
	}

	/// <summary>
	/// Write Helper for directly setting a node allocation within a specific SpellData/SkillTreeLedger.
	/// Creates the SkillTreeLedger if needed.
	/// </summary>
	public void SetAllocatedPoints(string spellID, string skillNodeID, int skillPoints)
	{
		SkillTreeLedger skillTreeLedger = GetOrCreateSkillTreeLedger(spellID);
		
		skillTreeLedger?.SetAllocatedSkillPoints(skillNodeID, skillPoints);
	}
}
