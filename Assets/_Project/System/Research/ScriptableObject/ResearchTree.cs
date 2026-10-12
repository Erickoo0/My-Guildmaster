using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Static Blueprint for the Guild Research Tree.
/// Holds a list of ResearchNodes.
/// </summary>
[CreateAssetMenu(fileName = "Research_Tree_Guild", menuName = "Research/Research Tree")]
public class ResearchTree : ScriptableObject
{
	[Header("Editor Visualization")]
	[SerializeField] private Sprite _backgroundImage;
	[SerializeField] private Vector2 _backgroundDimensions = new Vector2(500f, 550f); // Match your in-game UI RectTransform size

	[Header("Research Nodes")]
	[SerializeField] private List<ResearchNode> _researchNodesList = new List<ResearchNode>();

	private Dictionary<string, ResearchNode> _researchNodeDictionary;
	public Sprite BackgroundImage => _backgroundImage;
	public Vector2 BackgroundDimensions => _backgroundDimensions;
	public string ID { get; private set; } = "research_tree";
	public IReadOnlyList<ResearchNode> ResearchNodesList => _researchNodesList;

	private void OnValidate() => ValidateNodeIDs();

	private void SetupDictionary()
	{
		// Safety Check
		if (_researchNodeDictionary != null && _researchNodeDictionary.Count == _researchNodesList.Count)
			return;

		_researchNodeDictionary = new Dictionary<string, ResearchNode>();

		foreach (ResearchNode researchNode in _researchNodesList)
		{
			if (researchNode != null && !string.IsNullOrEmpty(researchNode.ID))
				_researchNodeDictionary[researchNode.ID] = researchNode;
		}
	}

	/// <summary>
	/// Searches the ResearchTree for a ResearchNode with the given ID.
	/// </summary>
	public ResearchNode GetResearchNodeByID(string researchNodeID)
	{
		// 1. if unable to find the ID
		if (string.IsNullOrEmpty(researchNodeID))
			return null;

		SetupDictionary();

		// 2. Search the dictionary for the specific node that matches the given ID
		if (_researchNodeDictionary.TryGetValue(researchNodeID, out ResearchNode researchNode))
			return researchNode;

		return null;
	}

	public bool ContainsNode(string researchNodeID) => GetResearchNodeByID(researchNodeID) != null;

	/// <summary>
	/// Checks if the prequisites for the given research node ID are met.
	/// </summary>
	public bool ArePrequisitesMet(string researchNodeID, ResearchLedger researchLedger)
	{
		ResearchNode researchNode = GetResearchNodeByID(researchNodeID);
		if (researchNode == null)
			return false;

		return researchNode.CheckPrerequisitesMet(researchLedger);
	}

	/// <summary>
	/// Checks if the given ResearchNode can be unlocked.
	/// This method only checks tree rules. ResearchManager checks global rules like research points.
	/// </summary>
	public bool CanUnlockResearchNode(string researchNodeID, ResearchLedger researchLedger)
	{
		// 1. Check if the ledger exists
		if (researchLedger == null)
			return false;

		// 2. Check if the ResearchNode exists in the research tree
		ResearchNode researchNode = GetResearchNodeByID(researchNodeID);
		if (researchNode == null)
			return false;

		// 3. Check if the ResearchNode is already unlocked
		if (researchLedger.CheckResearchNodeUnlocked(researchNodeID))
			return false;

		// 4. Check if ResearchNode has all prerequisites met
		return researchNode.CheckPrerequisitesMet(researchLedger);
	}

	/// <summary>
	/// Editor safety check for missing or duplicate ResearchNode IDs
	/// </summary>
	private void ValidateNodeIDs()
	{
		if (_researchNodesList == null)
			return;

		HashSet<string> seenIDs = new HashSet<string>();

		foreach (ResearchNode researchNode in _researchNodesList)
		{
			if (researchNode == null)
				continue;

			if (string.IsNullOrWhiteSpace(researchNode.ID))
			{
				Debug.LogWarning($"ResearchTree: Missing ID for ResearchNode {researchNode.DisplayName}");
				continue;
			}

			if (!seenIDs.Add(researchNode.ID))
				Debug.LogWarning($"ResearchTree: Duplicate ID for ResearchNode {researchNode.DisplayName}");
		}
	}
}
