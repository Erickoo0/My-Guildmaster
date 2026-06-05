using UnityEngine;

[System.Serializable]
public abstract class BaseAttackState : BaseActionState
{
    [Header("Attack Meta Data")] 
    [SerializeField] protected string attackID;
    protected SpellData attackData;
    
    [Header("Cast Bar")]
    protected CastBar castBar;
    protected bool isCasting;

    protected bool hasTriggered = false;
    protected float defaultDetectionLostRange;
    
    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);

        if (controller.globalSpellDatabase != null)
            attackData = controller.globalSpellDatabase.GetSpell<SpellData>(attackID);
        
        castBar = controller.GetComponent<CastBar>();
    }

    public override void Enter()
    {
        hasTriggered = false;
        isCasting = false;

        // Safety Check
        if (attackData == null)
        {
            Debug.LogWarning($"AttackData is null for {controller.gameObject.name}");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        defaultDetectionLostRange = controller.DetectionLostRange;
        controller.DetectionLostRange = defaultDetectionLostRange * 4f;

        controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.StartSpellAnimation(attackData.AnimationTag);
        controller.EntityAnimator.animator.Update(0f); // Force transition
        
        // Use the helper method to cleanly grab the timing
        float eventTime = GetAnimationEventTime();
        
        // --- Data-Driven Cast Logic ---
        if (attackData.baseCastTime > 0)
        {
            isCasting = true;
            
            castBar?.BeginCast(attackData.baseCastTime, attackID);
            
            float castSpeedMultiplier = eventTime / attackData.baseCastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else
        {
            controller.EntityAnimator.animator.speed = 1f;
        }
    }

    public override void Update()
    {
        
        if (hasTriggered && isCasting)
        {
            isCasting = false;
            castBar?.StopCast();
        }
        
        if (!controller.EntityAnimator.animator.GetBool(attackData.AnimationTag))
            stateMachine.ChangeState(controller.IdleState);
        
    }
    
    public override void Exit()
    {
        isCasting = false;
        castBar?.StopCast();

        controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool(attackData.AnimationTag, false);
        controller.EntityAnimator.RequestAnimationCancel();
        
        controller.EntityAnimator.animator.Update(0f);
        
        controller.DetectionLostRange = defaultDetectionLostRange;
        
        if (hasTriggered)
            controller.SetActionCooldown();
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
    
    protected abstract void HandleAnimationEvent();

    // --- Helper Methods ---

    private float GetAnimationEventTime()
    {
        var clipInfo = controller.EntityAnimator.animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length == 0) return 0f;
        
        AnimationClip clip = clipInfo[0].clip;
        
        foreach (var eventInfo in clip.events)
        {
            if (eventInfo.functionName == "RequestAnimationEvent") 
            {
                return eventInfo.time;
            }
        }
        
        // Default to full clip length if there is no event
        return clip.length; 
    }
}