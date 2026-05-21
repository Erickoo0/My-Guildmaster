using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
}
