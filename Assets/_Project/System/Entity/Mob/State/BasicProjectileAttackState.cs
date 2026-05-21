using System.Linq;
using UnityEngine;

[System.Serializable]
public class BasicProjectileAttackState : BaseCastState
{
    [Header("References")]
    [SerializeField] private string attackID;
    private ProjectileSpellData _spellData;
    private GameObject _firePoint;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _spellData = controller.GetAttackData<ProjectileSpellData>(attackID);
        
        var firePointComponent = controller.GetComponentInChildren<FirePoint>();
        _firePoint = (firePointComponent != null) ? firePointComponent.gameObject : controller.gameObject;
    }

    public override void Enter()
    {
        base.Enter();
        
        if (_spellData == null)
        {
            Debug.LogWarning("Attack data is null");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    
        controller.EntityAnimator.OnAnimationEventRequested += ExecuteAttack;
        controller.EntityMover.SetMoveDirection(Vector2.zero);

        StartCastingRoutine(_spellData.baseCastTime, _spellData.spellID);
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
        
        GameObject projectile = Object.Instantiate(_spellData.spellPrefab, spawnPosition, Quaternion.identity);
        projectile.transform.localScale *= _spellData.spellScale;
        
        if (projectile.TryGetComponent(out Projectile projectileComponent))
        {
            DamageData finalDamage = _spellData.CreateDamageData(controller.gameObject);
            
            projectileComponent.Setup(direction, _spellData.projectileSpeed, _spellData.projectileLifetime, finalDamage);
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
