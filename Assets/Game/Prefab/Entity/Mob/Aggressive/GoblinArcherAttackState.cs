using UnityEngine;

public class GoblinArcherAttackState : BaseActionState
{
    private ProjectileAttackData arrowData;
    private bool hasFired = false;
    
    public override void Setup(EntityController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        arrowData = controller?.GetAttackData<ProjectileAttackData>();
    }
    
    public override void Enter()
    {
        hasFired = false;
        
        if (arrowData == null)
        {
            Debug.LogWarning("Arrow data is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        controller.EntityAnimator.OnAnimationEventRequested += FireArrow;

        
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        controller.EntityAnimator.animator.SetBool("IsAttacking", true);
    }

    public override void Update()
    {
        if (controller.EntityAnimator.animator.GetBool("IsAttacking") == false)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    }

    private void FireArrow()
    {
        if (hasFired) return;
        
        // Safety Check
        if (controller.currentTarget == null)
        {
            Debug.LogWarning("No target to fire arrow at");
            return;
        }
        
        Vector2 spawnPosition = controller.transform.position;
        Vector2 targetPosition = controller.currentTarget.transform.position;
        Vector2 direction = (targetPosition - spawnPosition).normalized;
        
        GameObject arrow = Object.Instantiate(arrowData.attackPrefab, spawnPosition, Quaternion.identity);

        if (arrow.TryGetComponent(out Projectile projectile))
        {
            // Pass the DamageData from AttackData onto the projectile prefab (because we need to change source and hit direction)
            DamageData damageDataInstance = arrowData.damageData;
            damageDataInstance.source = controller.gameObject;
            damageDataInstance.hitDirection = direction;
            
            projectile.Setup(direction, arrowData.projectileSpeed, arrowData.projectileLifetime, damageDataInstance);
        }
        
        hasFired = true;
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= FireArrow;
    }
    
}
