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

	[Header("Barrage Settings")]
	[Tooltip("How many volleys to shoot in a row.")]
	[field: SerializeField] public int BarrageCount { get; private set; } = 1;
	[Tooltip("Time in seconds between each volley in the barrage.")]
	[field: SerializeField] public float BarrageDelay { get; private set; } = 0.2f;

	[Header("Hitbox Settings")]
	[field: SerializeField] public int MaxEnemiesHit { get; private set; } = 1;
	[field: SerializeField] public bool HitOncePerTarget { get; private set; } = true;
	[field: SerializeField] public bool DestroyOnMaxHits { get; private set; } = true;

	[Header("Impact EffectList List")]
	[SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();
	private EntityControllerBase _entityController;

	private FirePoint _firepoint;

	public override List<Effect> GetNestedEffects() => EffectsList;

	public override bool Execute(EffectPayload effectPayload)
	{
		if (Prefab == null) return false;

		// 1. Set up default straight line curve if null (once, before spawning)
		if (ProjectileCurve == null || ProjectileCurve.length == 0)
			ProjectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// 2. Cache the components
		_entityController = effectPayload.User.GetComponent<EntityControllerBase>();
		_firepoint = effectPayload.User.GetComponentInChildren<FirePoint>();

		// 2. If its a barrage, call a repeating sequence, passing in the Action to repeat
		if (BarrageCount > 1)
		{
			_entityController.StartCoroutine(RunEffectSequence(BarrageCount, BarrageDelay, (index) => SpawnVolley(effectPayload, index > 0)));
			return true;
		}

		// 3. If its not a barrage, simply execute projectile spawn once.
		return SpawnVolley(effectPayload, false);
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

	/// <summary>
	/// Handles the actual math and spawning of a single volley/spread.
	/// Recalculates position every time so moving entities update their fire points mid-barrage.
	/// </summary>
	private bool SpawnVolley(EffectPayload effectPayload, bool isBonusHit)
	{
		// 1. Set the spawn position
		Vector3 spawnPosition = _firepoint != null ? _firepoint.transform.position : effectPayload.User.transform.position;

		// 2. Calculate aim direction 
		Vector3 baseDirection = (effectPayload.TargetPosition - spawnPosition).normalized;
		if (baseDirection == Vector3.zero) baseDirection = Vector3.right;
		float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x)*Mathf.Rad2Deg;

		// 3. Calculate travel distance
		float distance = Vector3.Distance(spawnPosition, effectPayload.TargetPosition);
		if (distance < 1f) distance = 100f;

		// 4. Spawn projectile logic
		bool anySpawned = false;
		int projectileCount = Mathf.Max(1, Mathf.RoundToInt(ProjectileCount));

		// 5. Loop through projectile count
		for (int i = 0; i < projectileCount; i++)
		{
			// 6. Adjust the angle for each looped projectile
			float offsetAngle = projectileCount == 1
				? 0f
				: ((float)i/(projectileCount - 1) - 0.5f)*SpreadAngle;

			float projectileAngleDeg = baseAngle + offsetAngle;
			Vector3 projectileDirection = new Vector3(
				Mathf.Cos(projectileAngleDeg*Mathf.Deg2Rad),
				Mathf.Sin(projectileAngleDeg*Mathf.Deg2Rad),
				0f
				).normalized;
			Vector3 rotatedTargetPosition = spawnPosition + projectileDirection*distance;

			// 7. Spawn the projectile and apply scale
			GameObject projectileInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity);
			projectileInstance.transform.localScale *= Scale;

			// 8. Bundle the data and pass it to the projectile
			if (projectileInstance.TryGetComponent(out Projectile projectileComponent))
			{
				CombatContext combatContext = new CombatContext
				{
					User = effectPayload.User,
					EffectsList = EffectsList,
					SkillDataInstance = effectPayload.SkillDataInstance,
					SkillDamageBase = effectPayload.SkillDamageBase,
					IsBonusHit = isBonusHit
				};

				HitBoxSettings hitBoxSettings = new HitBoxSettings
				{
					MaxEnemiesHit = MaxEnemiesHit,
					HitOncePerTarget = HitOncePerTarget,
					DestroyOnMaxHits = DestroyOnMaxHits,
					HitImpact = effectPayload.HitImpact
				};

				ProjectileFlightData projectileFlightData = new ProjectileFlightData
				{
					TargetPosition = rotatedTargetPosition,
					Speed = Speed,
					Duration = Duration,
					MaxHeight = ProjectileHeight,
					Curve = ProjectileCurve,
					Scale = Scale
				};

				projectileComponent.Setup(combatContext, hitBoxSettings, projectileFlightData);
				anySpawned = true;
			}
		}

		return anySpawned;
	}
}
