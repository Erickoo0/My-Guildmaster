using UnityEngine;
/// <summary>
/// Defines any entity that can provide dialogue data to the DialogueManager.
/// </summary>
public interface ISpeaker
{
	string DialogueName { get; }
	Sprite DialoguePortrait { get; }
	DialogueGroup CurrentDialogueGroup { get; }
}
