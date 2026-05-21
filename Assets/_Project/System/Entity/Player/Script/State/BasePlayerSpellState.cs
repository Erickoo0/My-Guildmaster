using UnityEngine;

[System.Serializable]
public abstract class BasePlayerSpellState : State<PlayerController>
{
    [SerializeField] protected string spellID;
    
    protected SpellData spellData;
    protected bool _hasTriggered = false;

    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        spellData = controller?.GetAttackData<SpellData>(spellID);
    }

    public override void Enter()
    {
        _hasTriggered = false;

        if (spellData == null)
        {
            Debug.LogWarning($"Ability data for {spellID} is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        controller.EntityAnimator.OnAnimationEventRequested += HandleAnimationEvent;
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.animator.SetBool("IsAttacking", true);
        
        // Animation Speed Logic                        
        float clipDuration = controller.EntityAnimator.animator.GetCurrentAnimatorStateInfo(0).length;
        if (spellData.baseCastTime > 0)
        {
            float castSpeedMultiplier = clipDuration / spellData.baseCastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else
        {
            controller.EntityAnimator.animator.speed = 1f;
        }
    }

    public override void Update()
    {
        if (!controller.EntityAnimator.animator.GetBool("IsAttacking"))
            stateMachine.ChangeState(controller.IdleState);
    }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= HandleAnimationEvent;
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        
        controller.SetActionCooldown();
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
    
    // Child classes should implement this method to handle the attack logic
    protected abstract void HandleAnimationEvent();
}
