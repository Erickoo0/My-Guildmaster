using UnityEngine;

public interface IStateOverrider
{
    bool EvaluateRequirements();
    DialogueNode GetDialogue();
    string[] GetSpeechBubbles();
}
