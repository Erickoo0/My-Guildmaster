using UnityEngine;
/// <summary>
/// Manages the research system.
/// Holds ResearchTree, ResearchLedger, etc,
/// </summary>
public class ResearchManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private ResearchTree _researchTree;

	private ResearchLedger _researchLedger;
	public static ResearchManager Instance { get; private set; }

	public ResearchTree ResearchTree => _researchTree;
	public ResearchLedger ResearchLedger => _researchLedger;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		SetupLedger();
	}

	private void Start()
	{
		ReapplyUnlockedResearchRewards();
		EventBus.RequestResearchChanged();
	}

	private void SetupLedger()
	{
		if (_researchTree == null)
		{
			Debug.LogWarning("ResearchManager: No ResearchTree assigned.");
			return;
		}

		_researchLedger = new ResearchLedger(_researchTree.ID);
	}

	/// <summary>
	/// Checks if a ResearchNode can be unlocked.
	/// </summary>
	public bool CanUnlockResearchNode(string researchNodeID)
	{
		// Safety Check
		if (_researchTree == null || _researchLedger == null)
			return false;

		// 1. Check if the ResearchNode can be unlocked
		if (!_researchTree.CanUnlockResearchNode(researchNodeID, _researchLedger))
			return false;

		// 2. Check if the ResearchNode exists in the research tree
		ResearchNode researchNode = _researchTree.GetResearchNodeByID(researchNodeID);
		if (researchNode == null)
			return false;

		return true;
	}

	/// <summary>
	/// Attempts to unlock a ResearchNode and apply its rewards
	/// </summary>
	public bool TryUnlockResearchNode(string researchNodeID)
	{
		// 1. Check if unlockable
		if (!CanUnlockResearchNode(researchNodeID))
			return false;

		// 2. Get the ResearchNode from the ResearchTree
		ResearchNode researchNode = _researchTree.GetResearchNodeByID(researchNodeID);
		if (researchNode == null)
			return false;

		// 3. Unlock the node
		_researchLedger.UnlockResearchNode(researchNodeID);

		// 4. Apply the rewards
		researchNode.ApplyRewards();

		// 5. Notify observers
		EventBus.RequestResearchChanged();

		return true;
	}

	/// <summary>
	/// Reapplies rewards from all unlocked research nodes.
	/// Useful after loading saved games.
	/// </summary>
	public void ReapplyUnlockedResearchRewards()
	{
		if (_researchTree == null || _researchLedger == null)
			return;

		foreach (string researchNodeID in _researchLedger.UnlockedResearchNodeIDs)
		{
			ResearchNode researchNode = _researchTree.GetResearchNodeByID(researchNodeID);
			if (researchNode == null)
				continue;

			// Only apply reward if the reward is persistent
			foreach (ResearchReward reward in researchNode.RewardsList)
				if (reward != null && reward.IsPersistent)
					reward.ApplyReward();
		}
	}
}
