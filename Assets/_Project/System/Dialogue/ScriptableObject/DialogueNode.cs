using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class DialogueNode
{
    public string nodeID;
    public string[] dialogueLines;
    public DialogueOption[] dialogueOptions;
    public string nodeEvent; // Future, if we want to play animations or events when the a specific dialogue node starts
    
    [Header("Selection Criteria")]
    public List<RequirementData> requirements = new List<RequirementData>();
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