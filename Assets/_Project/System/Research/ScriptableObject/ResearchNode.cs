using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Static Blueprint for a ResearchNode in the ResearchTree.
/// Contains node metadata, prerequisites, and rewards.
/// </summary>
[Serializable]
public class ResearchNode
{
	[Header("Metadata")]
	[SerializeField] private string _id;
	[SerializeField] private string _displayName;
	[SerializeField] private Sprite _icon;
	[SerializeField, TextArea(3, 10)] private string _description;

	[Header("UI Position")]
	[SerializeField] private Vector2 _position;

	[Header("Prerequisites")]
	[SerializeField] private float _researchTime = 10f;
	[SerializeField] private List<ResearchNodePrerequisite> _prerequisitesList = new List<ResearchNodePrerequisite>();

	[Header("Rewards")]
	[SerializeReference, SubclassSelector] private List<ResearchReward> _rewardsList = new List<ResearchReward>();

	public string ID => _id;
	public string DisplayName => _displayName;
	public Sprite Icon => _icon;
	public string Description => _description;
	public Vector2 Position => _position;
	public float ResearchTime => _researchTime;
	public IReadOnlyList<ResearchNodePrerequisite> PrerequisitesList => _prerequisitesList;
	public IReadOnlyList<ResearchReward> RewardsList => _rewardsList;

	public bool CheckPrerequisitesMet(ResearchLedger researchLedger)
	{
		// Safety Check
		if (researchLedger == null)
			return false;

		// 1. Check if all prerequisites are met
		if (_prerequisitesList == null || _prerequisitesList.Count == 0)
			return true;
		foreach (ResearchNodePrerequisite prerequisite in _prerequisitesList)
		{
			if (prerequisite == null)
				continue;
			// If any prequisites are not met, return false
			if (!prerequisite.CheckPrerequisites(researchLedger))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Checks if this node is a prequisite for another node of the given ResearchNodeID.
	/// Useful for refund or dependency validation.
	/// </summary>
	public bool HasPrerequisites(string prequisiteNodeID)
	{
		if (string.IsNullOrEmpty(prequisiteNodeID))
			return false;
		if (_prerequisitesList == null)
			return false;

		foreach (ResearchNodePrerequisite prerequisite in _prerequisitesList)
		{
			if (prerequisite != null && prerequisite.RequiredResearchNodeID == prequisiteNodeID)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Applies all rewards associated with this research node.
	/// </summary>
	public void ApplyRewards()
	{
		if (_rewardsList == null)
			return;

		foreach (ResearchReward researchReward in _rewardsList)
		{
			if (researchReward == null)
				continue;

			researchReward.ApplyReward();
		}
	}
}
