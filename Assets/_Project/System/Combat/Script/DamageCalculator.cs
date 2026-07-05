using UnityEngine;
public enum DamageScalingStat
{
	AttackPower,
	AttackPowerFire,
	AttackPowerWater,
	AttackPowerEarth,
	AttackPowerLightning,
	AttackPowerAir,
	AttackPowerHoly,
	AttackPowerDark
}

/// <summary>
/// Centralized source of truth for all combat damage math.
/// All damage calculations should flow through here so formulas live in one place.
/// </summary>
public static class DamageCalculator
{
	/// <summary>
	/// Returns the value of the requested scaling stat from the user
	/// </summary>
	public static float GetScalingStat(DamageScalingStat scalingStat, IStatProvider statProvider)
	{
		// 1. If EntityStats is null, return 0
		if (statProvider?.EntityStats == null)
		{
			Debug.Log("DamageCalculator: EntityStats is null");
			return 0f;
		}

		// 2. Return the requested scaling stat
		return scalingStat switch
		{
			DamageScalingStat.AttackPower => statProvider.EntityStats.AttackPower,
			DamageScalingStat.AttackPowerFire => statProvider.EntityStats.AttackPowerFire,
			DamageScalingStat.AttackPowerWater => statProvider.EntityStats.AttackPowerWater,
			DamageScalingStat.AttackPowerEarth => statProvider.EntityStats.AttackPowerEarth,
			DamageScalingStat.AttackPowerLightning => statProvider.EntityStats.AttackPowerLightning,
			DamageScalingStat.AttackPowerHoly => statProvider.EntityStats.AttackPowerHoly,
			DamageScalingStat.AttackPowerDark => statProvider.EntityStats.AttackPowerDark,
			_ => 0f
		};
	}

	/// <summary>
	/// Computes the skill's scaled damage for this CAST. (Multi Hit skills will only need to call this method once at the start)
	/// Base skill damage + (scaling stat * scaling ratio)
	/// Called from SkillCastState
	/// </summary>
	public static float ComputeBaseSkillDamage(SkillDataInstance skillDataInstance, IStatProvider statProvider)
	{
		// Safety Check
		if (skillDataInstance == null) return 0f;

		// Compute the Base Damage
		float damageBase = skillDataInstance.DamageBase;
		float damageScalingStat = GetScalingStat(skillDataInstance.DamageScalingStat, statProvider);
		float damageScalingRatio = skillDataInstance.DamageScalingRatio;

		return damageBase + damageScalingStat*damageScalingRatio;
	}

	/// <summary>
	/// Applies the user's damage multiplier stat to the scaled damage.
	/// </summary>
	public static float ApplyDamageMultiplier(float damage, IStatProvider userStatProvider)
	{
		if (userStatProvider?.EntityStats == null) return damage;

		return damage*userStatProvider.EntityStats.DamageMultiplier;
	}

	/// <summary>
	/// Applies the target's defense to the damage.
	/// </summary>
	public static float ApplyDefense(float damage, IStatProvider targetStatProvider)
	{
		if (targetStatProvider?.EntityStats == null) return damage;

		// Defense is a simple flat reduction for now.
		return Mathf.Max(1f, damage - targetStatProvider.EntityStats.Defense);
	}

	/// <summary>
	/// Single entry point for final damage resolution.
	/// Applies multipliers, defense, and future resistances.
	/// </summary>
	public static float CalculateFinalDamage(float rawDamage, DamageType damageType, GameObject user, GameObject target)
	{
		// 1. Apply the user's damage multiplier
		if (user != null && user.TryGetComponent(out IStatProvider userStatProvider))
			rawDamage = ApplyDamageMultiplier(rawDamage, userStatProvider);

		// 2. Apply the targets defense
		if (target != null && target.TryGetComponent(out IStatProvider targetStatProvider))
			rawDamage = ApplyDefense(rawDamage, targetStatProvider);

		return rawDamage;
	}
}
