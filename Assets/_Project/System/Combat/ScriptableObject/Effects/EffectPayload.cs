using System.Collections.Generic;
using UnityEngine;
public class EffectPayload
{

	public HashSet<IDamagable> HitTargets;

	public EffectPayload(GameObject user, GameObject target = null, Vector3 targetPosition = default, Vector2 hitDirection = default, Vector2 hitImpactPoint = default, HashSet<IDamagable> hitTargets = null)
	{
		User = user;
		Target = target;
		TargetPosition = targetPosition;
		HitDirection = hitDirection;
		HitImpactPoint = hitImpactPoint;
		HitTargets = hitTargets;
	}
	public GameObject User { get; private set; }
	public GameObject Target { get; private set; }
	public Vector3 TargetPosition { get; private set; }

	public Vector2 HitDirection { get; private set; }
	public Vector2 HitImpactPoint { get; private set; }

	/// <summary>
	/// Hit impact intensity for hit pause and screen shake on successful hits.
	/// Set by the skill state before executing effects. Defaults to 0 (no impact feedback).
	/// </summary>
	public float HitImpact { get; set; }

	/// <summary>
	/// The SkillDataInstance this effect was triggered from.
	/// Used for scaling, skill stats, and conditional logic.
	/// </summary>
	public SkillDataInstance SkillDataInstance { get; set; }

	/// <summary>
	/// Pre-computed base skill damage for this cast.
	/// Shared across all effects in the chain, including nested and projectile effects.
	/// </summary>
	public float SkillDamageBase { get; set; }
}
