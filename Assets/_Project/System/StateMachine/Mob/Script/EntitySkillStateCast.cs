using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class EntitySkillStateCast : EntitySkillStateBase
{
	protected override void HandleAnimationEvent()
	{
		if (HasTriggered) return;
		if (controller == null) return;

		Vector3 casterPosition = controller.transform.position;
		TryUpdateAttackDirection();
		Vector2 castDirection = SkillDirection;

		Vector3 targetPosition = controller.CurrentTarget != null ? controller.CurrentTarget.position : casterPosition + (Vector3)castDirection;

		// 1. Construct payload and compute SkillDamageBase
		EffectPayload initialCastPayload = new EffectPayload(controller.gameObject)
		{
			Target = controller.gameObject,
			TargetPosition = targetPosition,
			HitDirection = SkillDirection,
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

		// 3. Apply Recoil if necessary
		if (SkillDataInstance.Animation == AnimationBool.IsAttackingStrong)
			controller.EntityMover.ApplyRecoil(castDirection);

		// 4. Apply VFX
		if (SkillDataInstance.Prefab != null)
		{
			Vector3 spawnPosition = controller.SkillController.FirePoint != null
				? controller.SkillController.FirePoint.transform.position
				: casterPosition;

			float relativeX = targetPosition.x - spawnPosition.x;

			Quaternion spawnRotation = relativeX >= 0
				? Quaternion.Euler(0, 0, 0)
				: Quaternion.Euler(0, 180, 0);

			GameObject spellVFX = Object.Instantiate(SkillDataInstance.Prefab, spawnPosition, spawnRotation, controller.transform);
			Object.Destroy(spellVFX, 1f);
		}

		HasTriggered = true;
	}
}
