using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class EffectSpawnProjectile : Effect
{
	[Header("Projectile Prefab")]
	[field: SerializeField] public GameObject Prefab { get; private set; }
	[field: SerializeField] public float Scale { get; private set; } = 1f;

	[Header("Flight Physics")]
	[field: SerializeField] public float Speed { get; private set; } = 12f;
	[field: SerializeField] public float Duration { get; private set; } = 3f;
	[field: SerializeField] public float ProjectileHeight { get; private set; } = 0f;
	[field: SerializeField] public AnimationCurve ProjectileCurve { get; private set; }

	[Header("Multi-Projectile")]
	[Tooltip("Number of projectiles to spawn. Rounded to nearest integer at runtime.")]
	[field: SerializeField] public float ProjectileCount { get; private set; } = 1f;
	[Tooltip("Total arc spread in degrees. Projectiles are evenly distributed across this arc, centered on the aim direction.")]
	[field: SerializeField] public float SpreadAngle { get; private set; } = 0f;

	[Header("Hitbox Settings")]
	[field: SerializeField] public int MaxEnemiesHit { get; private set; } = 1;
	[field: SerializeField] public bool HitOncePerTarget { get; private set; } = true;
	[field: SerializeField] public bool DestroyOnMaxHits { get; private set; } = true;

	[Header("Impact EffectList List")]
	[SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();

	public override List<Effect> GetNestedEffects() => EffectsList;

	public override bool Execute(EffectPayload effectPayload)
	{
		if (Prefab == null) return false;

		// 1. Find the user's firepoint component if they have one
		FirePoint firepoint = effectPayload.User.GetComponentInChildren<FirePoint>();
		Vector3 spawnPosition = firepoint != null ? firepoint.transform.position : effectPayload.User.transform.position;

		// 2. Set up default straight line curve if null (once, before spawning)
		if (ProjectileCurve == null || ProjectileCurve.length == 0)
			ProjectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// 3. Calculate base aim direction and angle from the payload's target position
		int count = Mathf.Max(1, Mathf.RoundToInt(ProjectileCount));
		Vector3 baseDirection = (effectPayload.TargetPosition - spawnPosition).normalized;
		if (baseDirection == Vector3.zero) baseDirection = Vector3.right;
		float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x)*Mathf.Rad2Deg;

		// 4. Preserve the original travel distance for all projectiles in the spread
		float distance = Vector3.Distance(spawnPosition, effectPayload.TargetPosition);
		if (distance < 1f) distance = 100f;

		// 5. Spawn projectiles in an arc pattern
		bool anySpawned = false;
		for (int i = 0; i < count; i++)
		{
			// Evenly distribute projectiles across the spread arc, centered on the aim direction.
			// For count=1 the offset is 0 (straight shot). For count=3, spread=30: offsets are -15, 0, +15.
			float offsetAngle = count == 1
				? 0f
				: ((float)i/(count - 1) - 0.5f)*SpreadAngle;

			// Compute the rotated direction and a far target position along it
			float projectileAngleDeg = baseAngle + offsetAngle;
			Vector3 projectileDirection = new Vector3(
				Mathf.Cos(projectileAngleDeg*Mathf.Deg2Rad),
				Mathf.Sin(projectileAngleDeg*Mathf.Deg2Rad),
				0f
				).normalized;
			Vector3 rotatedTargetPosition = spawnPosition + projectileDirection*distance;

			// 6. Spawn the projectile
			GameObject projectileInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity);

			// 7. Apply scale
			if (Scale != 1f) projectileInstance.transform.localScale *= Scale;

			// 8. Pass the data to the projectile
			if (projectileInstance.TryGetComponent(out Projectile projectileComponent))
			{
				projectileComponent.Setup(
					rotatedTargetPosition,
					Speed,
					Duration,
					ProjectileCurve,
					ProjectileHeight,
					effectPayload.User,
					EffectsList,
					MaxEnemiesHit,
					HitOncePerTarget,
					DestroyOnMaxHits,
					Scale
					);

				anySpawned = true;
			}
		}

		return anySpawned;
	}

	public override Effect Clone()
	{
		// Clone the nested On Hit EffectsList list
		List<Effect> clonedOnHitEffects = new List<Effect>();
		if (EffectsList != null)
			foreach (Effect effect in EffectsList)
				if (effect != null)
					clonedOnHitEffects.Add(effect.Clone());

		return new EffectSpawnProjectile
		{
			Prefab = Prefab,
			Scale = Scale,
			Speed = Speed,
			Duration = Duration,
			ProjectileHeight = ProjectileHeight,
			ProjectileCurve = ProjectileCurve != null ? new AnimationCurve(this.ProjectileCurve.keys) : null,
			MaxEnemiesHit = MaxEnemiesHit,
			HitOncePerTarget = HitOncePerTarget,
			DestroyOnMaxHits = DestroyOnMaxHits,
			ProjectileCount = ProjectileCount,
			SpreadAngle = SpreadAngle,
			EffectsList = clonedOnHitEffects
		};
	}
}
