using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class BaseNPCOverrideWanderState : BaseNPCWanderState, IStateOverrider
{
    [SerializeReference, SubclassSelector]
    public List<Requirement> requirements = new List<Requirement>();
    
    [Header("Override Dialogue Data")]
    [SerializeField] private DialogueGroup dialogueGroup;
    [SerializeField] private string[] speechBubbleDialogue;
    [SerializeField] private int priority = 10; // Higher priority means it will be evaluated first
    
    public int Priority => priority;
    
    public DialogueGroup GetDialogueGroup() => dialogueGroup;
    public string[] GetSpeechBubbles() => speechBubbleDialogue;

    public virtual bool EvaluateRequirements()
    {
        // If there are no requirements assigned, keep the state dormant.
        if (requirements == null || requirements.Count == 0) return false;

        foreach (Requirement req in requirements)
        {
            // Safety check in case an empty slot exists in the Inspector list
            if (req == null) continue; 

            // If even ONE requirement fails, the whole state fails to override
            if (!req.IsMet()) return false;
        }

        // All requirements passed! Time to override.
        return true;
    }

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
