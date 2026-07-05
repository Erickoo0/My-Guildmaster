using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public class EffectSpawnExplosion : Effect
{
	[Header("Explosion Settings")]
	[field: SerializeField] public GameObject Prefab { get; private set; }
	[field: SerializeField] public float Duration { get; private set; } = 0.5f;
	[field: SerializeField] public float Scale { get; private set; } = 1f;

	[Header("Explosion Impact Settings")]
	[SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();

	public override List<Effect> GetNestedEffects() => EffectsList;

	public override bool Execute(EffectPayload effectPayload)
	{
		if (Prefab == null) return false;

		// 1. Determine spawn location based on the payload's impact context
		Vector3 spawnPosition = effectPayload.HitImpactPoint != Vector2.zero
			? (Vector3)effectPayload.HitImpactPoint
			: effectPayload.TargetPosition;

		// 2. Spawn the explosion
		GameObject explosionInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity);
		explosionInstance.transform.localScale *= Scale;
		Object.Destroy(explosionInstance, Duration);

		// 3. Bundle and Pass the data
		if (explosionInstance.TryGetComponent(out HitBoxAOE hitBox))
		{
			CombatContext combatContext = new CombatContext
			{
				User = effectPayload.User,
				EffectsList = EffectsList,
				SkillDataInstance = effectPayload.SkillDataInstance,
				SkillDamageBase = effectPayload.SkillDamageBase
			};

			HitBoxSettings hitBoxSettings = new HitBoxSettings
			{
				MaxEnemiesHit = 999, // AOEs typically hit everything in radius
				HitOncePerTarget = true,
				DestroyOnMaxHits = false, // Lifetime handles destruction, not hit count
				HitImpact = effectPayload.HitImpact,
				InheritedTargetsList = effectPayload.HitTargets // Pass the memory chain!
			};

			hitBox.Setup(combatContext, hitBoxSettings);
			hitBox.EnableHitBox = true;
			return true;
		}

		return false;
	}

	public override Effect Clone()
	{
		List<Effect> clonedExplosionEffects = new List<Effect>();
		if (EffectsList != null)
			foreach (Effect effect in EffectsList)
				if (effect != null)
					clonedExplosionEffects.Add(effect.Clone());

		return new EffectSpawnExplosion
		{
			Prefab = Prefab,
			Duration = Duration,
			Scale = Scale,
			EffectsList = clonedExplosionEffects
		};
	}
}
