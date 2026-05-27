using System.Linq;
using UnityEngine;

[System.Serializable]
public class BasicProjectileAttackState : BaseAttackState
{
    private ProjectileSpellData _projectileAttackData;
    private GameObject _firePoint;
    
    [SerializeField] private AnimationCurve _projectileCurve;
    
    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _projectileAttackData = attackData as ProjectileSpellData;
        
        var firePointComponent = controller.GetComponentInChildren<FirePoint>();
        _firePoint = (firePointComponent != null) ? firePointComponent.gameObject : controller.gameObject;
        
        if (_projectileCurve == null || _projectileCurve.length == 0)
            _projectileCurve = CreateDefaultArcCurve();
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

        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        // Face the aim direction
        if (controller.currentTarget != null)
        {
            Vector2 aimDirection = (controller.currentTarget.transform.position - _firePoint.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
        }
    }
    
    protected override void HandleAnimationEvent()
    {
        // Safety Check
        if (hasTriggered) return;
        if (controller.currentTarget == null) return;
        
        // Calculate the flight direction of the projectile
        Vector2 spawnPosition = _firePoint.transform.position;
        Vector2 targetPosition = controller.currentTarget.position;
        
        GameObject projectile = Object.Instantiate(_projectileAttackData.spellPrefab, spawnPosition, Quaternion.identity);
        
        // Apply scale
        if (attackData.spellScale != 1f)
            projectile.transform.localScale *= _projectileAttackData.spellScale;
        
        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData finalDamage = _projectileAttackData.CreateDamageData(controller.gameObject);
            projectileComponent.Setup(targetPosition, _projectileAttackData.projectileSpeed, _projectileAttackData.projectileLifetime, _projectileCurve, _projectileAttackData.projectileHeight, _projectileAttackData.destroyOnMaxHits, finalDamage);
        }
        
        hasTriggered = true;
    }
    
    private AnimationCurve CreateDefaultArcCurve ()
    {
        return new AnimationCurve
            (
            new Keyframe(0f, 0f, 0f, 4f), 
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -4f, 0f)
            );
    }
}
