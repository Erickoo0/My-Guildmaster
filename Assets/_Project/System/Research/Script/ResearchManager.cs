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
	public ResearchNode ActiveResearchNode { get; private set; }

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

	private void Update()
	{
		// 1. Check if we are actively researching
		if (!_researchLedger.IsResearching || ActiveResearchNode == null)
			return;

		// 2. Tick the timer
		float researchWorkMultiplier = WorkerManager.Instance.GetWorkerWorkMultiplier(WorkerAssignmentType.Research);
		_researchLedger.AddResearchProgress(Time.deltaTime*researchWorkMultiplier);

		// 4. Check for completion
		if (_researchLedger.CurrentResearchProgress >= ActiveResearchNode.ResearchTime)
			CompleteResearch(ActiveResearchNode);
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
	public bool CanUnlockResearchNode(string researchNodeID) => _researchTree.CanUnlockResearchNode(researchNodeID, _researchLedger);

	/// <summary>
	/// Attempts to unlock a ResearchNode and apply its rewards
	/// </summary>
	public bool TryStartResearch(string researchNodeID)
	{
		// 1. Check if unlockable
		if (!CanUnlockResearchNode(researchNodeID))
			return false;

		// 2. Check if already researching 
		if (_researchLedger.IsResearching)
		{
			Debug.Log($"ResearchManager: Already researching another node");
			return false;
		}

		// 3. Set the active research
		ActiveResearchNode = _researchTree.GetResearchNodeByID(researchNodeID);
		_researchLedger.SetActiveResearch(researchNodeID);

		// 4. Notify observers
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

	private void CompleteResearch(ResearchNode researchNode)
	{
		_researchLedger.UnlockResearchNode(researchNode.ID);
		_researchLedger.ClearActiveResearch();
		researchNode.ApplyRewards();
		EventBus.RequestResearchChanged();
	}
}
