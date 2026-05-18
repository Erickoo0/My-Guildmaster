using UnityEngine;

public abstract class BasseNPCOverrideState : State<NPCController>, IStateOverrider
{
    [Header("Override Dialogue Data")]
    [SerializeField] private DialogueNode dialogueNode;
    [SerializeField] private string[] speechBubbleDialogue;
    
    public DialogueNode GetDialogue() => dialogueNode;
    public string[] GetSpeechBubbles() => speechBubbleDialogue;

    public abstract bool EvaluateRequirements();

    protected void FinishOverride()
    {
        controller.ClearOverrideState();
    }

}
