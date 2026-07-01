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
	[field: SerializeField] public GameObject User { get; private set; }
	[field: SerializeField] public GameObject Target { get; private set; }
	[field: SerializeField] public Vector3 TargetPosition { get; private set; }

	[field: SerializeField] public Vector2 HitDirection { get; private set; }
	[field: SerializeField] public Vector2 HitImpactPoint { get; private set; }

	/// <summary>
	/// Hit impact intensity for hit pause and screen shake on successful hits.
	/// Set by the skill state before executing effects. Defaults to 0 (no impact feedback).
	/// </summary>
	public float HitImpact { get; set; }
}
