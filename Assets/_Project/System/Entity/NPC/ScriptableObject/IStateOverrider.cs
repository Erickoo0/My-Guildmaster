using UnityEngine;

public interface IStateOverrider
{
    int Priority { get; }
    bool EvaluateRequirements();
    DialogueGroup GetDialogueGroup();
    string[] GetSpeechBubbles();
}
