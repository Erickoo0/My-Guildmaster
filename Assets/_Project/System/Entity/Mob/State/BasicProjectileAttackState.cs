using System.Linq;
using UnityEngine;

[System.Serializable]
public class BasicProjectileAttackState : BaseCastState
{
    [Header("References")]
    [SerializeField] private string attackID;
    private ProjectileAttackData _attackData;
    private GameObject _firePoint;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _attackData = controller.GetAttackData<ProjectileAttackData>(attackID);
        
        var firePointComponent = controller.GetComponentInChildren<FirePoint>();
        _firePoint = (firePointComponent != null) ? firePointComponent.gameObject : controller.gameObject;
    }

    public override void Enter()
    {
        base.Enter();
        
        if (_attackData == null)
        {
            Debug.LogWarning("Attack data is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    
        controller.EntityAnimator.OnAnimationEventRequested += ExecuteAttack;
        controller.EntityMover.SetMoveDirection(Vector2.zero);

        StartCastingRoutine(_attackData.baseCastTime, _attackData.attackID);
    }

    private void ExecuteAttack()
    {
        // Safety Check
        if (controller.currentTarget == null) return;
        
        if (hasFired) return;
        
        // Calculate the flight direction of the projectile
        Vector2 spawnPosition = _firePoint.transform.position;
        Vector2 targetPosition = controller.currentTarget.transform.position;
        Vector2 direction = (targetPosition - spawnPosition).normalized;
        
        GameObject projectile = Object.Instantiate(_attackData.attackPrefab, spawnPosition, Quaternion.identity);
        projectile.transform.localScale *= _attackData.attackScale;
        
        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData finalDamage = _attackData.CreateDamageData(controller.gameObject);
            
            projectileComponent.Setup(direction, _attackData.projectileSpeed, _attackData.projectileLifetime, finalDamage);
        }
        
        hasFired = true;
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= ExecuteAttack;
        base.Exit();
    }
}
