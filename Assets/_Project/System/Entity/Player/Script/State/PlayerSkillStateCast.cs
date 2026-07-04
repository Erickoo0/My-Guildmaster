using System;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class PlayerSkillStateCast : PlayerSkillStateBase
{
	public override void Enter()
	{
		// Face the aim direction upon starting the cast
		Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
		controller.EntityAnimator.FaceDirection(aimDirection);
		controller.EntityAnimator.animator.Update(0f);

		base.Enter();
	}

	public override void Update()
	{
		base.Update();

		// Face the target while winding up
		if (!HasTriggered)
		{
			Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
			controller.EntityAnimator.FaceDirection(aimDirection);
		}
	}

	public override void Exit()
	{
		base.Exit();

		Vector2 aimDirection = (controller.WorldMousePosition - controller.transform.position).normalized;
		controller.EntityAnimator.FaceDirection(aimDirection);
		controller.EntityAnimator.animator.Update(0f);
	}

	protected override void HandleAnimationEvent()
	{
		if (HasTriggered) return;
		if (controller == null) return;

		Vector3 casterPosition = controller.transform.position;
		Vector2 castDirection = (controller.WorldMousePosition - casterPosition).normalized;

		// 1. Create a primary payload describing the INITIAL CAST event
		EffectPayload initialCastPayload = new EffectPayload(
			user: controller.gameObject,
			target: controller.gameObject,                 // Default target is caster for instant self-effects
			targetPosition: controller.WorldMousePosition, // Target position is where the mouse is pointing
			hitDirection: castDirection,
			hitImpactPoint: casterPosition
			);
		initialCastPayload.HitImpact = SkillDataInstance.HitImpact;

		// 2. Compute the skill's base damage once per cast and pass it with the entire effect chain
		initialCastPayload.SkillDataInstance = SkillDataInstance;
		initialCastPayload.SkillDamageBase = DamageCalculator.ComputeBaseSkillDamage(SkillDataInstance, controller.GetComponent<IStatProvider>());

		// 3. Execute all skill effects
		if (SkillDataInstance.EffectsList != null && SkillDataInstance.EffectsList.Count > 0)
			foreach (Effect effect in SkillDataInstance.EffectsList)
				effect.Execute(initialCastPayload);

		// 4. Apply Recoil & Screen shake if necessary
		controller.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		if (SkillDataInstance.Animation == AnimationBool.IsAttackingStrong)
			controller?.EntityMover.ApplyRecoil(castDirection);

		// 5. Apply VFX
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

		HasTriggered = true;
	}
}
