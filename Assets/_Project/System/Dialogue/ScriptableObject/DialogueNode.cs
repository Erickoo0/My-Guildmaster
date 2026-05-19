using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogue System/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    public string[] dialogueLines;
    public DialogueOption[] dialogueOptions;
    public string nodeEvent; // Future, if we want to play animations or events when the a specific dialogue node starts
    
    [Header("Selection Criteria")]
    public List<Requirement> requirements;
    [Range(0, 100)] public float selectionWeight = 10f;
    [Tooltip("If true and requirements are met, this node is guaranteed to be picked.")]
    public bool isImportant = false;

}

[System.Serializable]
public class DialogueOption
{
    public string optionName;
    public DialogueNode nextNode;
    
    [Header("Event Data")]
    public string dialogueEvent;
    public string eventParameter; // For quests, put the QuestID here.
}