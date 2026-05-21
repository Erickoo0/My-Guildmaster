using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class BaseNPCOverrideWanderState : BaseNPCWanderState, IStateOverrider
{
    public List<RequirementData> requirements = new List<RequirementData>();
    
    [Header("Override Dialogue Data")]
    [SerializeField] private DialogueGroup dialogueGroup;
    [SerializeField] private string[] speechBubbleDialogue;
    [SerializeField] private int priority = 10; // Higher priority means it will be evaluated first
    
    public int Priority => priority;
    
    public DialogueGroup GetDialogueGroup() => dialogueGroup;
    public string[] GetSpeechBubbles() => speechBubbleDialogue;

    public abstract bool EvaluateRequirements();

    // Override the base method to prevent going into Idle upon reaching destination
    protected override void OnReachedDestination()
    {
        controller.EntityAnimator.FaceDirection((_selectedPOI.lookDirection));
        _arrivedMainDestination = true;
    }
    
    protected void FinishOverride()
    {
        controller.ClearOverrideState();
    }
}
