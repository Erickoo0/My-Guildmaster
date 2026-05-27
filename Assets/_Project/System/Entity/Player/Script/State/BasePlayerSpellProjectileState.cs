using UnityEngine;

[System.Serializable]
public class BasePlayerSpellProjectileState : BasePlayerSpellState
{
   private ProjectileSpellData _spellProjectileData;
   private GameObject _firePoint;

   [SerializeField] private AnimationCurve _projectileCurve;

   public override void Setup(PlayerController controller, StateMachine stateMachine)
   {
      base.Setup(controller, stateMachine);
      
      _spellProjectileData = spellData as ProjectileSpellData;
      _firePoint = controller?.GetComponentInChildren<FirePoint>()?.gameObject;

      if (_projectileCurve == null || _projectileCurve.length == 0)
         _projectileCurve = CreateDefaultArcCurve();
   }

   public override void Enter()
   {
      // Safety Check
      if (_spellProjectileData == null || _spellProjectileData.spellPrefab == null || _firePoint == null)
      {
         Debug.LogWarning("Missing Projectile Data or FirePoint!");
         stateMachine.ChangeState(controller.IdleState);
         return;
      }
      
      // Face the aim direction
      Vector2 aimDirection = (controller.WorldMousePosition - _firePoint.transform.position).normalized;
      controller.EntityAnimator.FaceDirection(aimDirection);
      controller.EntityAnimator.animator.Update(0f);
      
      base.Enter();
   }

   public override void Exit()
   {
      base.Exit();
      
      Vector2 aimDirection = (controller.WorldMousePosition - _firePoint.transform.position).normalized;
      controller.EntityAnimator.FaceDirection(aimDirection);
      controller.EntityAnimator.animator.Update(0f);

   }

   protected override void HandleAnimationEvent()
   {
      if (hasTriggered) return;
      if (controller == null) return;
      
      Vector3 spawnPosition = _firePoint.transform.position;
      Vector2 direction = (controller.WorldMousePosition - spawnPosition).normalized;
      
      GameObject projectile = Object.Instantiate(_spellProjectileData.spellPrefab, spawnPosition, Quaternion.identity);
      
      // Apply Scale
      if (spellData.spellScale != 1f) projectile.transform.localScale *= spellData.spellScale;
      
      if (projectile.TryGetComponent(out Projectile projectileComponent))
      {
         DamageData finalDamage = _spellProjectileData.CreateDamageData(controller.gameObject);
         projectileComponent.Setup(controller.WorldMousePosition, _spellProjectileData.projectileSpeed, _spellProjectileData.projectileLifetime, _projectileCurve, _spellProjectileData.projectileHeight, finalDamage);
      }
      
      // Apply Recoil
      if (spellData.spellAnimation == AnimationBool.IsAttackingStrong) 
      {
         controller?.EntityMover.ApplyRecoil(direction);

      }
      
      // Consume Mana
      controller?.mpComponent.ConsumeMp(spellData.baseMpCost);
      
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
