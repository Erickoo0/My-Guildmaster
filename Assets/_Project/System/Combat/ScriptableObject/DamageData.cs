using System;
using UnityEngine;

public enum DamageType { Physical, Fire, Water, Earth, Lightning, Holy, Shadow }

[Serializable]
public struct DamageData
{
	public float Amount;
	public Vector2 Direction;
	public Vector2 ImpactPoint;
	public float KnockbackForce;
	public float KnockbackDuration;
	public float KnockbackHeight;
	public DamageType Type;
	public GameObject Source;

	// When true, bypasses HurtBox invulnerability
	public bool BonusHit;

	public DamageData(
		float amount, Vector2 direction, Vector2 impactPoint, float force,
		float duration, float height, DamageType type, GameObject source, bool bonusHit = false
	)
	{
		Amount = amount;
		Direction = direction;
		ImpactPoint = impactPoint;
		KnockbackForce = force;
		KnockbackDuration = duration;
		KnockbackHeight = height;
		Type = type;
		Source = source;
		BonusHit = bonusHit;
	}
}
