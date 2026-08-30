using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class PlayerSkillStateCast : PlayerSkillStateBase
{
	// Cache the aim direction formula so we don't have to rewrite it everywhere
	private Vector2 AimDirection => (controller.WorldMousePosition - controller.transform.position).normalized;

	public override void Enter()
	{
		// Face the aim direction upon starting the cast
		controller.EntityAnimator.FaceDirection(AimDirection);
		controller.EntityAnimator.animator.Update(0f);

		base.Enter();
	}

	public override void Update()
	{
		base.Update();

		// Face the target while winding up
		if (!HasTriggered)
			controller.EntityAnimator.FaceDirection(AimDirection);
	}

	public override void Exit()
	{
		base.Exit();

		controller.EntityAnimator.FaceDirection(AimDirection);
		controller.EntityAnimator.animator.Update(0f);
	}

	protected override void HandleAnimationEvent()
	{
		if (HasTriggered) return;
		if (controller == null) return;

		Vector3 casterPosition = controller.transform.position;
		Vector2 castDirection = AimDirection;

		// 1. Construct payload and compute SkillDamageBase
		EffectPayload initialCastPayload = new EffectPayload(controller.gameObject)
		{
			Target = controller.gameObject,
			TargetPosition = controller.WorldMousePosition,
			HitDirection = AimDirection,
			HitImpactPoint = casterPosition,
			HitImpact = SkillDataInstance.HitImpact,
			HitTargets = new HashSet<IDamagable>(),
			SkillDataInstance = SkillDataInstance,
			SkillDamageBase = DamageCalculator.ComputeBaseSkillDamage(SkillDataInstance, controller.StatProvider)
		};

		// 2. Execute all skill effects
		if (SkillDataInstance.EffectsList != null && SkillDataInstance.EffectsList.Count > 0)
			foreach (Effect effect in SkillDataInstance.EffectsList)
				effect.Execute(initialCastPayload);

		// 3. Apply Recoil & Screen shake if necessary
		controller.CinemachineImpulseSource.GenerateImpulse();
		if (SkillDataInstance.Animation == AnimationBool.IsAttackingStrong)
			controller?.EntityMover.ApplyRecoil(castDirection);

		// 4. Apply VFX
		if (SkillDataInstance.Prefab != null)
		{
			Vector3 spawnPosition = controller.SkillController.FirePoint != null
				? controller.SkillController.FirePoint.transform.position
				: casterPosition;

			float angle = Mathf.Atan2(castDirection.y, castDirection.x)*Mathf.Rad2Deg;

			Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);

			GameObject spellVFX = Object.Instantiate(SkillDataInstance.Prefab, spawnPosition, spawnRotation, controller.transform);
			Object.Destroy(spellVFX, 1f);
		}


		// 6. Consume Mana
		controller?.MpComponent.ConsumeMp(SkillDataInstance.MpCost);

		// 7. Set cooldown
		HasTriggered = true;
		controller.SkillController.TriggerSkillCooldown(SkillDataInstance.ID);
	}
}
