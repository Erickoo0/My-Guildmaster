using System;
using UnityEngine;
[Serializable]
public class EffectDealDamage : Effect
{
	[field: SerializeField] public float Amount { get; private set; }
	[field: SerializeField] public DamageType Type { get; private set; }
	[field: SerializeField] public float KnockbackForce { get; private set; } = 10f;
	[field: SerializeField] public float KnockbackDuration { get; private set; } = 0.2f;
	[field: SerializeField] public float KnockbackHeight { get; private set; } = 0.1f;

	public override bool Execute(EffectPayload effectPayload)
	{

		if (effectPayload.Target == null) return false;

		if (effectPayload.Target.TryGetComponent(out IDamagable target))
		{
			DamageData data = new DamageData(
				Amount,
				effectPayload.HitDirection,   // Passed from Hitbox physics
				effectPayload.HitImpactPoint, // Passed from Hitbox physics
				KnockbackForce,
				KnockbackDuration,
				KnockbackHeight,
				Type,
				effectPayload.User
				);

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
			KnockbackForce = KnockbackForce,
			KnockbackDuration = KnockbackDuration,
			KnockbackHeight = KnockbackHeight
		};
	}
}
