using UnityEngine;

[System.Serializable]
public class BasePlayerSpellProjectileState : BasePlayerSpellState
{
   private ProjectileSpellData _spellProjectileData;
   private GameObject _firePoint;

   public override void Setup(PlayerController controller, StateMachine stateMachine)
   {
      base.Setup(controller, stateMachine);
      
      _spellProjectileData = spellData as ProjectileSpellData;
      _firePoint = controller?.GetComponentInChildren<FirePoint>()?.gameObject;
   }

   public override void Enter()
   {
      base.Enter();
      
      if (_spellProjectileData == null || _spellProjectileData.spellPrefab == null || _firePoint == null)
      {
         Debug.LogWarning("Missing Projectile Data or FirePoint!");
         stateMachine.ChangeState(controller.IdleState);
         return;
      }
      
      // Face the aim direction
      Vector2 aimDirection = (controller.WorldMousePosition - _firePoint.transform.position).normalized;
      controller.EntityAnimator.FaceDirection(aimDirection);
   }

   protected override void HandleAnimationEvent()
   {
      if (_hasTriggered) return;
      
      Vector3 spawnPosition = _firePoint.transform.position;
      Vector2 direction = (controller.WorldMousePosition - spawnPosition).normalized;
      
      GameObject projectile = Object.Instantiate(_spellProjectileData.spellPrefab, spawnPosition, Quaternion.identity);
      
      if (projectile.TryGetComponent(out Projectile projectileComponent))
      {
         DamageData finalDamage = _spellProjectileData.CreateDamageData(controller.gameObject);
         projectileComponent.Setup(direction, _spellProjectileData.projectileSpeed, _spellProjectileData.projectileLifetime, finalDamage);
      }
      
      _hasTriggered = true;
   }
}
