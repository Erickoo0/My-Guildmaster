using UnityEngine;

[System.Serializable]
public abstract class BaseAttackState : BaseActionState
{
    [Header("Attack Meta Data")] 
    [SerializeField] protected string attackID;
    protected SkillData attackDataSource;
    protected SkillDataInstance attackDataInstance;
    protected Vector2 attackDirection;
    public float SelectionWeight => attackDataSource != null ? attackDataSource.SelectionWeight : 0f;
    
    [Header("Cast Bar")]
    protected CastBar castBar;
    protected bool isCasting;

    protected bool hasTriggered = false;
    protected float defaultDetectionLostRange;
    
    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);

        // 1. Get the attack data
        if (controller._spellController.GlobalSkillDatabase != null)
        {
            attackDataSource = controller._spellController.GlobalSkillDatabase.GetSkillDataByID<SkillData>(attackID);
            if (attackDataSource != null)
                attackDataInstance = attackDataSource.CreateSpellDataInstance();
        }
        
        if (attackDataSource == null)
        {
            Debug.LogWarning($"AttackData is null for {controller.gameObject.name}");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        // 2. Get the cast bar
        castBar = controller.GetComponent<CastBar>();
    }

    public override void Enter()
    {
        controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
        
        hasTriggered = false;
        isCasting = false;
        
        // 1. Change to EntityMover
        if (controller.aiLerp != null)
        {
            controller.aiLerp.canSearch = false;
            controller.aiLerp.canMove = false;
            controller.aiLerp.destination = controller.transform.position;
        }
        
        // Force Rigidbody velocity zero
        if (controller._rigidBody2D != null)
            controller._rigidBody2D.linearVelocity = Vector2.zero;
        
        // 2. Save default ranges
        defaultDetectionLostRange = controller.TargetLostRange;
        controller.TargetLostRange = defaultDetectionLostRange * 4f;
        
        // 3. Force look direction update
        TryUpdateAttackDirection();
        controller.EntityAnimator.FaceDirection(attackDirection);
        
        // 4. Start the animation
        controller.EntityAnimator.StartSpellAnimation(attackDataInstance.AnimationTag);
        controller.EntityAnimator.animator.Update(0f); // Force transition
        
        // 5. Use the helper method to cleanly grab the timing
        float eventTime = GetAnimationEventTime();
        
        // 6. Casting logic
        if (attackDataInstance.DisplayCastBar)
        {
            isCasting = true;
            
            castBar?.BeginCast(attackDataInstance.CastTime, attackID);
            
            float castSpeedMultiplier = eventTime / attackDataInstance.CastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else
        {
            controller.EntityAnimator.animator.speed = 1f;
        }
    }

    public override void Update()
    {
        // 1. Disable cast bar after attack has triggered
        if (hasTriggered && isCasting)
        {
            isCasting = false;
            castBar?.StopCast();
        }
        
        // 2. Transition to Idle as soon as animation gets set to false via animationEnd event
        if (!controller.EntityAnimator.animator.GetBool(attackDataInstance.AnimationTag))
            stateMachine.ChangeState(controller.IdleState);
        
        // 3. Change to Chase State if knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack)
        {
            stateMachine.ChangeState(controller.ChaseState);
            return;
        }
        
        // 4. Face the target while winding up
        if (!hasTriggered)
            TryUpdateAttackDirection();
        controller.EntityAnimator.FaceDirection(attackDirection);
        
    }
    
    public override void Exit()
    {
        // 1. Disable castbar
        isCasting = false;
        castBar?.StopCast();

        // 2. Unsubscribe
        controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;
        
        // 3. Reset animation
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool(attackDataInstance.AnimationTag, false);
        controller.EntityAnimator.RequestAnimationCancel();
        controller.EntityAnimator.animator.Update(0f);
        
        // 4. Face direction 
        if (TryUpdateAttackDirection())
        {
            controller.EntityAnimator.FaceDirection(attackDirection);
            controller.EntityAnimator.animator.Update(0f);
        }
        
        // 5. Restore default range
        controller.TargetLostRange = defaultDetectionLostRange;
        
        // 6. Set cooldown
        if (hasTriggered)
            controller._spellController.SetActionCooldown();
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

    protected bool TryUpdateAttackDirection()
    {
        if (controller.currentTarget == null)
        {
            Debug.Log("BaseAttackState: Target has become null");
            return false;
        }
        
        attackDirection = ((Vector2)controller.currentTarget.position - (Vector2)controller.transform.position).normalized;
        
        return true;
    }

    public bool CheckRequirementsMet(GameObject context) => attackDataInstance != null && attackDataInstance.AreRequirementsMet(context);
}