using UnityEngine;

[System.Serializable]
public class SkeletonWarriorAttackState : BaseActionState
{
    [SerializeField] private string attackID;
    private MeleeAttackData _attackData;
    private HitBox _meleeHitbox;
    
    private float _defaultActionRange;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);

        _attackData = controller.GetAttackData<MeleeAttackData>(attackID);
        
        // Find the hitbox attached to the enemy
        _meleeHitbox = controller.GetComponentInChildren<HitBoxContact>(true);
        if (_meleeHitbox == null)
            Debug.Log("Could not find HitBoxContact on " + controller.gameObject.name + "");

        if (_meleeHitbox != null)
            _meleeHitbox.enableHitbox = false;;
        
    }

    public override void Enter()
    {
        if (_attackData == null || _meleeHitbox == null)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        controller.EntityAnimator.OnAnimationEventRequested += ActivateHitbox;
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.animator.SetBool("IsAttacking", true);
    }

    public override void Update()
    {
        if (!controller.EntityAnimator.animator.GetBool("IsAttacking"))
        {
            stateMachine.ChangeState(controller.IdleState);
        }
    }
    
    private void ActivateHitbox()
    {
        // Save the default action range, then increase action range during the attack
        _defaultActionRange = controller.ActionRange;
        controller.ActionRange = _defaultActionRange * 10f; // Large range so they commit to the attack
        
        // Calculate direction
        Vector2 direction = (controller.currentTarget.transform.position - controller.transform.position).normalized;
        
        // Setup Damage Data
        DamageData finalDamage = _attackData.CreateDamageData(controller.gameObject);
        
        _meleeHitbox.Setup(finalDamage);
        _meleeHitbox.enableHitbox = true; ;
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= ActivateHitbox;
        
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        
        if (_meleeHitbox != null)
            _meleeHitbox.enableHitbox = false;
        
        controller.ActionRange = _defaultActionRange;
        
        controller.SetActionCooldown();
        
        controller.currentTarget = null;
    }
}
