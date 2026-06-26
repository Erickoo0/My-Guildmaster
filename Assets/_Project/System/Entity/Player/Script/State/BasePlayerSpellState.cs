using UnityEngine;

[System.Serializable]
public abstract class BasePlayerSpellState : State<PlayerController>
{
    [Header("Spell Meta Data")]
    [SerializeField] protected string spellID;
    protected SkillData SkillDataSource;
    protected SkillDataInstance SkillDataInstance;
    [HideInInspector] public int CurrentSlotIndex = -1;
    
    public float MpCost => SkillDataInstance != null ? SkillDataInstance.MpCost : 0f;
    public SkillData GetSpellDataSource() => SkillDataSource;
    public SkillDataInstance GetSpellDataInstance() => SkillDataInstance;
    
    [Header("Cast Bar")]
    protected CastBar castBar;
    protected bool isCasting;
    
    protected bool hasTriggered = false;

    [Header("Spell Modification Testing")]
    [SerializeField] private ModifierSpellCollection debugModifierCollection;

    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        if (controller.spellController != null && controller.spellController.GlobalSkillDatabase != null)
        {
            SkillDataSource = controller.spellController.GlobalSkillDatabase.GetSkillDataByID<SkillData>(spellID);
            if (SkillDataSource != null)
            {
                SkillDataInstance = SkillDataSource.CreateSpellDataInstance();
                
                // Testing
                if (debugModifierCollection != null)
                    debugModifierCollection.ApplyAllModifiers(SkillDataInstance);
            }
        }
        else
            Debug.LogError("PlayerController.spellController.GlobalSkillDatabase is null");
        
        castBar = controller.spellController?.CastBar;
    }

    public override void Enter()
    {
        hasTriggered = false;
        isCasting = false;
        
        // Safety check
        if (SkillDataSource == null || CurrentSlotIndex == -1)
        {
            Debug.LogWarning($"spellInstance or CurrentSlotIndex is null for {controller.gameObject.name}");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.StartSpellAnimation(SkillDataInstance.AnimationTag);
        controller.EntityAnimator.animator.Update(0f); // Force transition
        
        // Get the exact time the event fires, rather than just clip length
        float eventTime = GetAnimationEventTime();
        
        // Cast Logic
        float castSpeedMultiplier = eventTime / SkillDataInstance.CastTime;
        controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        if (SkillDataInstance.DisplayCastBar)
        {
            isCasting = true;
            castBar?.BeginCast(SkillDataInstance.CastTime, SkillDataInstance.Name);
        }
    }

    public override void Update()
    {
        // Safety check
        if (SkillDataInstance == null || CurrentSlotIndex == -1) return;
        
        // 1. Check if the skill keybind is still being held
        if (!hasTriggered && isCasting)
        {
            if (!controller.spellController.IsSpellKeyHeld(CurrentSlotIndex))
            {
                castBar?.StopCast();
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
        }
        
        // 2. Stop the cast bar when the skill executes
        if (hasTriggered && isCasting)
        {
            isCasting = false;
            castBar?.StopCast();
        }
        
        if (!controller.EntityAnimator.animator.GetBool(SkillDataInstance.AnimationTag))
            stateMachine.ChangeState(controller.IdleState);
    }

    public override void Exit()
    {
        isCasting = false;
        castBar?.StopCast();
        
        controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool(SkillDataInstance.AnimationTag, false);
        
        if (hasTriggered)
            controller.spellController.SetActionCooldown();
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
    
    // Child classes should implement this method to handle the attack logic
    protected abstract void HandleAnimationEvent();
    
    //----Helper Methods----
    private float GetAnimationEventTime()
    {
        var clipInfo = controller.EntityAnimator.animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length == 0) return 0f;
        
        AnimationClip clip = clipInfo[0].clip;
        
        foreach (var eventInfo in clip.events)
        {
            // This string must perfectly match the method in EntityAnimator.cs
            if (eventInfo.functionName == "RequestAnimationEvent") 
            {
                return eventInfo.time;
            }
        }
        
        // Default to full clip length if you forgot to place an event on the timeline
        return clip.length; 
        
    }
}
