using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Runtime/Save Class. Holds a list of unlocked ResearchNode IDs.
/// Links unlocked node records to ResearchNode.ID in the ResearchTree.
/// </summary>
[Serializable]
public class ResearchLedger
{
	[SerializeField] private string _researchTreeID;
	[SerializeField] private List<string> _unlockedResearchNodeIDs;

	// Constructor
	public ResearchLedger(string researchTreeID)
	{
		_researchTreeID = researchTreeID;
		_unlockedResearchNodeIDs = new List<string>();
	}

	public string ResearchTreeID => _researchTreeID;
	public IReadOnlyList<string> UnlockedResearchNodeIDs => _unlockedResearchNodeIDs;

	/// <summary>
	/// Checks if the given ResearchNode has been unlocked.
	/// </summary>
	public bool CheckResearchNodeUnlocked(string researchNodeID)
	{
		// Safety Check
		if (string.IsNullOrWhiteSpace(researchNodeID) || _unlockedResearchNodeIDs == null)
			return false;

		return _unlockedResearchNodeIDs.Contains(researchNodeID);
	}

	/// <summary>
	/// Marks a ResearchNode as unlocked.
	/// ResearchManager and ResearchLedgerController will handle the rest.
	/// </summary>
	/// <param name="researchNodeID"></param>
	public void UnlockResearchNode(string researchNodeID)
	{
		// Safety Check
		if (string.IsNullOrWhiteSpace(researchNodeID))
			return;

		if (_unlockedResearchNodeIDs == null)
			_unlockedResearchNodeIDs = new List<string>();

		if (_unlockedResearchNodeIDs.Contains(researchNodeID))
			return;

		_unlockedResearchNodeIDs.Add(researchNodeID);
	}

	public int GetTotalUnlockedResearchNodes()
	{
		if (_unlockedResearchNodeIDs == null)
			return 0;

		return _unlockedResearchNodeIDs.Count;
	}
}
