using UnityEngine;

public interface IStateOverrider
{
    int Priority { get; }
    bool EvaluateRequirements();
    DialogueNode GetDialogue();
    string[] GetSpeechBubbles();
}
