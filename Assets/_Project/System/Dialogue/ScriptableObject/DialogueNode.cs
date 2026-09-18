using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueNode
{
	public string nodeID;
	[TextArea(3, 10)]
	public string[] dialogueLines;
	public DialogueOption[] dialogueOptions;
	[SerializeReference, SubclassSelector]
	public DialogueAction[] nodeEvents;

	[Header("Selection Criteria")]
	[SerializeReference, SubclassSelector]
	public List<Requirement> requirements = new List<Requirement>();
	[Range(0, 100)] public float selectionWeight = 10f;
	[Tooltip("If true and requirements are met, this node is guaranteed to be picked.")]
	public bool isImportant = false;
}

[Serializable]
public class DialogueOption
{
	public string optionName;

	[DialogueNodeLink]
	public string targetNodeID;

	[SerializeReference, SubclassSelector]
	public DialogueAction[] optionEvents;
}
