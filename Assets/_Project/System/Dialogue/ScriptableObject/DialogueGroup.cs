using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewDialogueGroup", menuName = "Dialogue System/Dialogue Group")]
public class DialogueGroup : ScriptableObject
{
	[SerializeField] private string startingNodeID = "Intro";
	[SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
	public List<DialogueNode> DialogueNodes => dialogueNodes;

	public DialogueNode GetStartingNode()
	{
		DialogueNode startNode = dialogueNodes.Find(n => n.nodeID == startingNodeID);

		if (startNode == null)
		{
			if (dialogueNodes.Count > 0)
			{
				Debug.LogWarning($"[DialogueGroup]: Starting Node ID '{startingNodeID}' not found in {name}. Falling back to element 0.");
				return dialogueNodes[0];
			}

			Debug.LogError($"[DialogueGroup]: {name} has absolutely no nodes inside its list!");
			return null;
		}

		return startNode;
	}

	public DialogueNode GetNodeByID(string id)
	{
		return dialogueNodes.Find(n => n.nodeID == id);
	}

    #if UNITY_EDITOR
	private void OnValidate()
	{
		if (dialogueNodes == null || dialogueNodes.Count == 0) return;

		HashSet<string> existingIDs = new HashSet<string>();

		foreach (DialogueNode node in dialogueNodes)
		{
			// 1. Check for missing IDs
			if (string.IsNullOrWhiteSpace(node.nodeID))
			{
				Debug.LogWarning($"[DialogueGroup: {name}] A node is missing an ID!");
			}
			// 2. Check for duplicate IDs
			else if (!existingIDs.Add(node.nodeID))
			{
				Debug.LogError($"[DialogueGroup: {name}] DUPLICATE ID FOUND: {node.nodeID}. This will break routing.");
			}

			// 3. Check for broken option links
			if (node.dialogueOptions != null)
			{
				foreach (DialogueOption option in node.dialogueOptions)
				{
					if (!string.IsNullOrEmpty(option.targetNodeID) && !NodeExists(option.targetNodeID))
					{
						Debug.LogError($"[DialogueGroup: {name}] Broken Link! Node '{node.nodeID}' has an option pointing to '{option.targetNodeID}', but that ID does not exist.");
					}
				}
			}
		}
	}

	private bool NodeExists(string id) => dialogueNodes.Exists(n => n.nodeID == id);
    #endif
}
