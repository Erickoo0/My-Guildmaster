using System.Linq;
using UnityEngine;

[System.Serializable]
public class GoblinArcherAttackState : BaseActionState
{
    [SerializeField] private string attackID;
    private ProjectileAttackData _attackData;
    private GameObject _firePoint;
    private bool _hasFired = false;

    private float _defaultActionRange;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _attackData = controller.GetAttackData<ProjectileAttackData>(attackID);
        
        var firePointComponent = controller.GetComponentInChildren<FirePoint>();
        
        _firePoint = (firePointComponent != null) ? firePointComponent.gameObject : controller.gameObject;
    }

    public override void Enter()
    {
        _hasFired = false;
        
        if (_attackData == null)
        {
            Debug.LogWarning("Arrow data is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        controller.EntityAnimator.OnAnimationEventRequested += FireProjectile;

        
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

    private void FireProjectile()
    {
        if (_hasFired) return;
        
        // Safety Check
        if (controller.currentTarget == null)
        {
            Debug.LogWarning("No target to fire arrow at");
            return;
        }

        // Save the default action range then Increase action range during the attack
        _defaultActionRange = controller.ActionRange;
        controller.ActionRange = _defaultActionRange * 1.5f;
        
        // Calculate the direction of the projectile
        Vector2 spawnPosition = _firePoint.transform.position;
        Vector2 targetPosition = controller.currentTarget.transform.position;
        Vector2 direction = (targetPosition - spawnPosition).normalized;
        
        GameObject projectile = Object.Instantiate(_attackData.attackPrefab, spawnPosition, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            // Pass the DamageData from AttackData onto the projectile prefab (because we need to change source and hit direction)
            DamageData damageDataInstance = _attackData.damageData;
            damageDataInstance.source = controller.gameObject;
            damageDataInstance.hitDirection = direction;
            
            projectileComponent.Setup(direction, _attackData.projectileSpeed, _attackData.projectileLifetime, damageDataInstance);
        }
        
        _hasFired = true;
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= FireProjectile;
        
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        
        controller.ActionRange = _defaultActionRange;
        
        controller.SetActionCooldown();
        
        controller.currentTarget = null;
    }
    
}
