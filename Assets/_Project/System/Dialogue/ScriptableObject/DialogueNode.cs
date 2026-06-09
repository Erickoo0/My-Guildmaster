using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class DialogueNode
{
    public string nodeID;
    public string[] dialogueLines;
    public DialogueOption[] dialogueOptions;
    public NodeEventData[] nodeEvents;
    
    [Header("Selection Criteria")]
    [SerializeReference, SubclassSelector]
    public List<Requirement> requirements = new List<Requirement>();
    [Range(0, 100)] public float selectionWeight = 10f;
    [Tooltip("If true and requirements are met, this node is guaranteed to be picked.")]
    public bool isImportant = false;

}

[System.Serializable]
public class DialogueOption
{
    public string optionName;
    public string targetNodeID;
    
    [Header("Event Data")]
    public string dialogueEvent;
    public string eventParameter; // For quests, put the QuestID here.
}

[System.Serializable]
public class NodeEventData
{
    public string eventName;
    public string eventParameter;
}