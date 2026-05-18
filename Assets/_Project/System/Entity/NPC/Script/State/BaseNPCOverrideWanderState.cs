using UnityEngine;

public abstract class BaseNPCOverrideWanderState : BaseNPCWanderState, IStateOverrider
{
    [Header("Override Dialogue Data")]
    [SerializeField] private DialogueNode dialogueNode;
    [SerializeField] private string[] speechBubbleDialogue;
    
    public DialogueNode GetDialogue() => dialogueNode;
    public string[] GetSpeechBubbles() => speechBubbleDialogue;

    public abstract bool EvaluateRequirements();

    // Override the base method to prevent going into Idle upon reaching destination
    protected override void OnReachedDestination()
    {
        
    }
    
    protected void FinishOverride()
    {
        controller.ClearOverrideState();
    }
}
