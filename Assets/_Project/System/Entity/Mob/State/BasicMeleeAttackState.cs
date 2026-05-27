using UnityEngine;

[System.Serializable]
public class BasicMeleeAttackState : BaseAttackState
{
    private MeleeSpellData _meleeSpellData;
    private HitBoxContact _meleeHitbox;
    
    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);

        _meleeSpellData = attackData as MeleeSpellData;
        
        // Find the hitbox attached to the enemy
        _meleeHitbox = controller.GetComponentInChildren<HitBoxContact>(true);
        if (_meleeHitbox == null)
            Debug.Log("Could not find HitBoxContact on " + controller.gameObject.name + "");

        if (_meleeHitbox != null)
            _meleeHitbox.enableHitbox = false;;
    }

    public override void Enter()
    {
        if (_meleeSpellData == null || _meleeHitbox == null)
        {
            Debug.LogWarning("Missing Melee Attack Data or Hitbox!");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }

        base.Enter();

        _meleeHitbox.enableHitbox = false;
    }

    public override void Update()
    {
        base.Update();
        
        // Face the target while winding up, stop tracking once we swing
        if (controller.currentTarget != null && !hasTriggered)
        {
            Vector2 aimDirection = (controller.currentTarget.transform.position - controller.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
        }
    }
    
    protected override void HandleAnimationEvent()
    { 
        // Safety Check
        if (hasTriggered) return;
        if (controller.currentTarget == null) return;
        
        // Setup Damage Data
        DamageData finalDamage = _meleeSpellData.CreateDamageData(controller.gameObject);
        _meleeHitbox.Setup(finalDamage);
        _meleeHitbox.enableHitbox = true; ;
        
        hasTriggered = true;
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        if (_meleeHitbox != null)
            _meleeHitbox.enableHitbox = false;

        base.Exit();
        //controller.currentTarget = null;
    }
}
