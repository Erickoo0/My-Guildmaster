using System;
using UnityEngine;
/// <summary>
/// Defines a ResearchNode requirement that must be met for a ResearchNode to be unlocked.
/// </summary>
[Serializable]
public class ResearchNodePrerequisite
{
	[SerializeField] private string _requiredResearchNodeID;

	public string RequiredResearchNodeID => _requiredResearchNodeID;

	/// <summary>
	/// Checks if the required research node is unlocked.
	/// </summary>
	public bool CheckPrerequisites(ResearchLedger researchLedger)
	{
		// Safety Check
		if (researchLedger == null)
			return false;

		if (string.IsNullOrWhiteSpace(_requiredResearchNodeID))
			return true;

		// Check if the required research node is unlocked
		return researchLedger.CheckResearchNodeUnlocked(_requiredResearchNodeID);
	}
}
