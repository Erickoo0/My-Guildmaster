using UnityEngine;

[System.Serializable]
public abstract class BaseCastState : BaseActionState
{
    protected CastBar castBar;
    protected bool isCasting;
    protected bool hasFired;
    protected float defaultActionRange;
    
    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        castBar = controller.GetComponent<CastBar>();
    }

    protected void StartCastingRoutine(float baseCastTime, string attackID)
    {
        hasFired = false;
        
        controller.EntityAnimator.animator.SetBool("IsAttacking", true);
        controller.EntityAnimator.animator.Update(0f); // Force transition
        
        // Get the animation length
        var clipInfo = controller.EntityAnimator.animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length == 0) return;
        AnimationClip clip = clipInfo[0].clip;
        
        // Get the length untill attack event called
        float eventTime = 0;
        foreach (var eventInfo in clip.events)
        {
            if (eventInfo.functionName == "RequestAnimationEvent") 
            {
                eventTime = eventInfo.time;
                break;
            }
        }
        
        // If there is no event, default to full animation length
        if (eventTime <= 0) eventTime = clip.length;
        
        if (baseCastTime > 0)
        {
            isCasting = true;
            castBar?.BeginCast(baseCastTime, attackID);
            
            float castSpeedMultiplier = eventTime / baseCastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else
        {
            isCasting = false;
            controller.EntityAnimator.animator.speed = 1f;
        }
    }

    public override void Enter()
    {
        // Save the default action range then Increase action range during the attack
        defaultActionRange = controller.ActionRange;
        controller.ActionRange = defaultActionRange * 4f;
    }

    public override void Update()
    {
        if (hasFired && isCasting)
        {
            isCasting = false;
            castBar?.StopCast();
        }
        
        if (!controller.EntityAnimator.animator.GetBool("IsAttacking"))
        {
            stateMachine.ChangeState(controller.IdleState);
        }
    }
    
    public override void Exit()
    {
        isCasting = false;
        castBar?.StopCast();
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        controller.ActionRange = defaultActionRange;
        //controller.currentTarget = null;
        
        controller.SetActionCooldown();
    }
}
