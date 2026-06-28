using UnityEngine;

/// <summary>
/// Static Blueprint for one dependency requirement for a SkillNode.
/// This class is stored in the SkillNode's _prerequisites list.
/// Evaluates against a SkillTreeLedger.'
/// </summary>
[System.Serializable]
public class SkillNodePrerequisite
{
	[SerializeField] private string _requiredSkillNodeID;

	[Min(1)]
	[SerializeField] private int _requiredSkillPoints = 1;

	public string RequiredSkillNodeID => _requiredSkillNodeID;
	public int RequiredSkillPoints => _requiredSkillPoints;

	/// <summary>
	/// Checks whether the required node has enough points allocated in the given SkillTreeLedger.
	/// </summary>
	public bool CheckRequiredSkillNodeIsMet(SkillTreeLedger ledger)
	{
		if (ledger == null)
			return false;

		// If there is no required node, return true
		if (string.IsNullOrWhiteSpace(_requiredSkillNodeID))
			return true;

		// Compare the ledger's allocated points to the required points'
		return ledger.GetAllocatedSkillPoints(_requiredSkillNodeID) >= _requiredSkillPoints;
	}
}
