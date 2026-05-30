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
    private bool isStateFinished = false; // Optional bool to permanently disable state after first override
    
    public DialogueGroup GetDialogueGroup() => dialogueGroup;
    public string[] GetSpeechBubbles() => speechBubbleDialogue;
    public int Priority => priority;

    public virtual bool EvaluateRequirements()
    {
        // If there are no requirements assigned, keep the state dormant.
        if (requirements == null || requirements.Count == 0) return false;

        // Loop through all requirements
        foreach (Requirement req in requirements)
        {
            // Safety check in case an empty slot exists in the Inspector list
            if (req == null) continue; 

            // If even ONE requirement fails, the whole state fails to override
            if (!req.IsMet()) return false;
        }
        
        // Check if the state has been permanently disabled
        if (isStateFinished) return false;

        // All requirements passed! Time to override.
        return true;
    }

    // Override the base method to prevent going into Idle upon reaching destination
    protected override void OnReachedDestination()
    {
        // 1. Check if POI is a teleporter
        if (!string.IsNullOrEmpty(_selectedPOI.TeleportPOI))
        {
            // 2. Ask the POI Registry for the associated GameObject of the string
            PointOfInterest teleportTarget = POIRegistry.GetPOIByID(_selectedPOI.TeleportPOI);
            
            // 3. Ensure the registry found the associated GameObject
            if (teleportTarget != null)
                controller.aiPath.Teleport(teleportTarget.transform.position);
            else
                Debug.LogWarning($"[{controller.gameObject.name}] Teleport failed! Could not find POI with ID: '{_selectedPOI.TeleportPOI}' in the POIRegistry.");
        }
        
        // Face Direction logic
        controller.EntityAnimator.FaceDirection((_selectedPOI.lookDirection));

        // Set the location
        controller.currentLocation = _selectedPOI.Location;
    }
    
    protected void FinishOverride()
    {
        controller.ClearOverrideState();
    }
}
