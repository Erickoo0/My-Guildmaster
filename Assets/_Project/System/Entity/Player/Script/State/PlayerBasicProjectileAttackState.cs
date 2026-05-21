using UnityEngine;

[System.Serializable]
public class PlayerBasicProjectileAttackState : State<PlayerController>
{
    [SerializeField] private string spellID;
    private ProjectileSpellData _spellData;
    private GameObject _firePoint;
    private bool _hasFired = false;
    
    public override void Setup(PlayerController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _spellData = controller?.GetAttackData<ProjectileSpellData>(spellID);
        
        _firePoint = controller?.GetComponentInChildren<FirePoint>().gameObject;
    }
    
    public override void Enter()
    {
        _hasFired = false;
        
        // Safety Check
        if (_spellData == null || _spellData.spellPrefab == null || _firePoint == null)
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
        
        // Get the duration of the animation 
        float clipDuration = controller.EntityAnimator.animator.GetCurrentAnimatorStateInfo(0).length;
        
        // Apply animation speed multiplier based on Cast Time
        if (_spellData.baseCastTime > 0)
        {
            float castSpeedMultiplier = clipDuration/_spellData.baseCastTime;
            controller.EntityAnimator.animator.speed = castSpeedMultiplier;
        }
        else // If cast time is 0, use default
        {
            controller.EntityAnimator.animator.speed = 1f;
        }
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
        GameObject projectile = Object.Instantiate(_spellData.spellPrefab, spawnPosition, Quaternion.identity);

        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData finalDamage = _spellData.CreateDamageData(controller.gameObject);
        
            projectileComponent.Setup(direction, _spellData.projectileSpeed, _spellData.projectileLifetime, finalDamage);
        }
    
        _hasFired = true;
        controller.SetActionCooldown();
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityAnimator.OnAnimationEventRequested -= FireProjectile;
        
        controller.EntityAnimator.animator.speed = 1f;
        controller.EntityAnimator.animator.SetBool("IsAttacking", false);
        
        controller.SetActionCooldown();
    }
}
