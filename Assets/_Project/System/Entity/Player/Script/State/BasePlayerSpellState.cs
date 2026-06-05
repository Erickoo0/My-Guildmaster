using UnityEngine;

[System.Serializable]
public abstract class BasePlayerSpellState : State<PlayerController>
{
    [Header("Spell Meta Data")]
    [SerializeField] protected string spellID;
    protected SpellData spellData;
    [HideInInspector] public int CurrentSlotIndex = -1;
    public float MpCost => spellData != null ? spellData.baseMpCost : 0f;
    public SpellData GetSpellData() => spellData;
    
    [Header("Cast Bar")]
    protected CastBar castBar;
    protected bool isCasting;
    
    protected bool hasTriggered = false;

    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        if (controller.globalSpellDatabase != null)
            spellData = controller.globalSpellDatabase.GetSpell<SpellData>(spellID);
        else
            Debug.LogError("PlayerController.globalSpellDatabase is null");
        
        castBar = controller?.GetComponent<CastBar>();
    }

    public override void Enter()
    {
        hasTriggered = false;
        isCasting = false;
        
        // Safety check
        if (spellData == null || CurrentSlotIndex == -1)
        {
            Debug.LogWarning($"spellData or CurrentSlotIndex is null for {controller.gameObject.name}");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.StartSpellAnimation(spellData.AnimationTag);
        controller.EntityAnimator.animator.Update(0f); // Force transition
        
        // Get the exact time the event fires, rather than just clip length
        float eventTime = GetAnimationEventTime();
        
        // Cast Logic
        if (spellData.baseCastTime > 0.5)
        {
            isCasting = true;
            
            castBar?.BeginCast(spellData.baseCastTime, spellData.spellName);
            
            float castSpeedMultiplier = eventTime / spellData.baseCastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else
        {
            controller.EntityAnimator.animator.speed = 1f;
        }
    }

    public override void Update()
    {
        // Safety check
        if (spellData == null || CurrentSlotIndex == -1) return;
        
        // 1. Check if the spell keybind is still being held
        if (!hasTriggered && isCasting)
        {
            if (!controller.IsSpellKeyHeld(CurrentSlotIndex))
            {
                castBar?.StopCast();
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
        }
        
        // 2. Stop the cast bar when the spell executes
        if (hasTriggered && isCasting)
        {
            isCasting = false;
            castBar?.StopCast();
        }
        
        if (!controller.EntityAnimator.animator.GetBool(spellData.AnimationTag))
            stateMachine.ChangeState(controller.IdleState);
    }

    public override void Exit()
    {
        isCasting = false;
        castBar?.StopCast();
        
        controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool(spellData.AnimationTag, false);
        
        if (hasTriggered)
            controller.SetActionCooldown();
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
