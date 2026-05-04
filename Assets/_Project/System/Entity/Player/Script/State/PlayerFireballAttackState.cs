using UnityEngine;

public class PlayerFireballAttackState : State<PlayerController>
{
    [SerializeField] private string attackID;
    private ProjectileAttackData _attackData;
    private GameObject _firePoint;
    private bool _hasFired = false;
    
    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _attackData = controller?.GetAttackData<ProjectileAttackData>(attackID);
        
        _firePoint = controller?.GetComponentInChildren<FirePoint>().gameObject;
    }
    
    public override void Enter()
    {
        _hasFired = false;
        
        // Safety Check
        if (_attackData == null || _attackData.attackPrefab == null || _firePoint == null)
        {
            Debug.LogWarning("Attack data is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        
        controller.EntityAnimator.OnAnimationEventRequested += FireProjectile;
        
        Vector2 aimDirection = (controller.WorldMousePosition - _firePoint.transform.position).normalized;
        controller.EntityAnimator.FaceDirection(aimDirection);
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
    
        Vector3 spawnPosition = _firePoint.transform.position;
    
        // 1. Calculate the direction once: Target - Origin
        Vector2 direction = (controller.WorldMousePosition - spawnPosition).normalized;
    
        // 2. Instantiate the projectile
        GameObject projectile = Object.Instantiate(_attackData.attackPrefab, spawnPosition, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData damageDataInstance = _attackData.damageData;
            damageDataInstance.source = controller.gameObject;
        
            // Use the direction we just calculated
            damageDataInstance.hitDirection = direction;
        
            projectileComponent.Setup(direction, _attackData.projectileSpeed, _attackData.projectileLifetime, damageDataInstance);
        }
    
        _hasFired = true;
        controller.SetActionCooldown();
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= FireProjectile;
        
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        
        controller.SetActionCooldown();
    }
}
