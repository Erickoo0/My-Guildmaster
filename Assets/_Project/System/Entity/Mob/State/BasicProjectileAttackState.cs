using System.Linq;
using UnityEngine;

[System.Serializable]
public class BasicProjectileAttackState : BaseAttackState
{
    private ProjectileSpellData _projectileAttackData;
    private GameObject _firePoint;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _projectileAttackData = attackData as ProjectileSpellData;
        
        var firePointComponent = controller.GetComponentInChildren<FirePoint>();
        _firePoint = (firePointComponent != null) ? firePointComponent.gameObject : controller.gameObject;
    }

    public override void Enter()
    {
        // Safety CHeck
        if (_projectileAttackData == null || _projectileAttackData.spellPrefab == null)
        {
            Debug.LogWarning("Missing Projectile Attack Data or Prefab!");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    
        // Face the aim direction
        if (controller.currentTarget != null)
        {
            Vector2 aimDirection = (controller.currentTarget.transform.position - _firePoint.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
            controller.EntityAnimator.animator.Update(0f);
        }

        base.Enter();
    }
    
    protected override void HandleAnimationEvent()
    {
        // Safety Check
        if (hasTriggered) return;
        if (controller.currentTarget == null) return;
        
        // Calculate the flight direction of the projectile
        Vector2 spawnPosition = _firePoint.transform.position;
        Vector2 targetPosition = controller.currentTarget.transform.position;
        Vector2 direction = (targetPosition - spawnPosition).normalized;
        
        GameObject projectile = Object.Instantiate(_projectileAttackData.spellPrefab, spawnPosition, Quaternion.identity);
        projectile.transform.localScale *= _projectileAttackData.spellScale;
        
        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData finalDamage = _projectileAttackData.CreateDamageData(controller.gameObject);
            projectileComponent.Setup(direction, _projectileAttackData.projectileSpeed, _projectileAttackData.projectileLifetime, finalDamage);
        }
        
        hasTriggered = true;
    }
}
