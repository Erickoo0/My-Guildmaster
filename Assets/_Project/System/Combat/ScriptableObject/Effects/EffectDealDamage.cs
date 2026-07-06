using System;
using UnityEngine;
[Serializable]
public class EffectDealDamage : Effect
{
	[Header("Damage Settings")]
	[field: SerializeField] public float Amount { get; private set; }
	[field: SerializeField] public DamageType Type { get; private set; }
	[field: SerializeField] public bool BonusHit { get; private set; } = false;
	[Tooltip("If true, uses the skill's pre-computed BaseSkillDamage instead of Amount.")]
	[field: SerializeField] public bool UseSkillBaseDamage { get; private set; } = true;
	[Tooltip("Multiplier on BaseSkillDamage. Useful for explosions for example")]
	[field: SerializeField] public float SkillDamageMultiplier { get; private set; } = 1f;

	[Header("Knockback Settings")]
	[field: SerializeField] public float KnockbackForce { get; private set; } = 10f;
	[field: SerializeField] public float KnockbackDuration { get; private set; } = 0.2f;
	[field: SerializeField] public float KnockbackHeight { get; private set; } = 0.1f;

	public override bool Execute(EffectPayload effectPayload)
	{
		// Safety Check
		if (effectPayload.Target == null) return false;

		if (effectPayload.Target.TryGetComponent(out IDamagable target))
		{
			float rawDamage = UseSkillBaseDamage
				? effectPayload.SkillDamageBase*SkillDamageMultiplier
				: Amount;

			DamageData data = new DamageData
			{
				Amount = rawDamage,
				Direction = effectPayload.HitDirection,
				ImpactPoint = effectPayload.HitImpactPoint,
				KnockbackForce = KnockbackForce,
				KnockbackDuration = KnockbackDuration,
				KnockbackHeight = KnockbackHeight,
				Type = Type,
				Source = effectPayload.User,
				IsBonusHit = BonusHit || effectPayload.IsBonusHit
			};

			target.TakeDamage(data);
			return true;
		}

		return false;
	}

	public override Effect Clone()
	{
		return new EffectDealDamage
		{
			Amount = Amount,
			Type = Type,
			BonusHit = BonusHit,
			UseSkillBaseDamage = UseSkillBaseDamage,
			SkillDamageMultiplier = SkillDamageMultiplier,

			KnockbackForce = KnockbackForce,
			KnockbackDuration = KnockbackDuration,
			KnockbackHeight = KnockbackHeight,
		};
	}
}
