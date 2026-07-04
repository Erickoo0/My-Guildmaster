using System;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class EntitySkillState : SkillStateBase
{
	protected override void HandleAnimationEvent()
	{
		if (HasTriggered) return;
		if (controller == null) return;

		Vector3 casterPosition = controller.transform.position;
		TryUpdateAttackDirection();
		Vector2 castDirection = SkillDirection;

		Vector3 targetPosition = controller.CurrentTarget != null ? controller.CurrentTarget.position : casterPosition + (Vector3)castDirection;

		// 1. Create a primary payload describing the INITIAL CAST event
		EffectPayload initialCastPayload = new EffectPayload(
			user: controller.gameObject,
			target: controller.gameObject,  // Default target is caster for instant self-effects
			targetPosition: targetPosition, // Target position is where the mouse is pointing
			hitDirection: castDirection,
			hitImpactPoint: casterPosition
			);

		// 2. Compute the skill's base damage once per cast and pass it with the entire effect chain
		initialCastPayload.SkillDataInstance = SkillDataInstance;
		initialCastPayload.SkillDamageBase = DamageCalculator.ComputeBaseSkillDamage(SkillDataInstance, controller.GetComponent<IStatProvider>());

		// 3. Execute all skill effects
		if (SkillDataInstance.EffectsList != null && SkillDataInstance.EffectsList.Count > 0)
			foreach (Effect effect in SkillDataInstance.EffectsList)
				effect.Execute(initialCastPayload);

		// 4. Apply Recoil if necessary
		if (SkillDataInstance.Animation == AnimationBool.IsAttackingStrong)
			controller.EntityMover.ApplyRecoil(castDirection);

		// 5. Apply VFX
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
