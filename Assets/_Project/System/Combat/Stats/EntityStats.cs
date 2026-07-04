using UnityEngine;
public class EntityStats : MonoBehaviour
{
	[Header("Movement")]
	public float MoveSpeedMultiplier = 1f;

	[Header("Combat")]
	public float DamageMultiplier = 1f;
	public float DamageTakenMultiplier = 1f;
	public float Defense = 0f;
	public float AttackPower = 10f;
	public float AttackPowerFire;
	public float AttackPowerWater;
	public float AttackPowerEarth;
	public float AttackPowerLightning;
	public float AttackPowerHoly;
	public float AttackPowerDark;

	[Header("Level Scaling")]
	public float DefenseBase;
	public float DefensePerLvl = 1f;
	public float AttackPowerBase = 10f;
	public float AttackPowerPerLvl = 2f;
	public float AttackPowerFireBase;
	public float AttackPowerFirePerLvl;
	public float AttackPowerWaterBase;
	public float AttackPowerWaterPerLvl;
	public float AttackPowerEarthBase;
	public float AttackPowerEarthPerLvl;
	public float AttackPowerLightningBase;
	public float AttackPowerLightningPerLvl;
	public float AttackPowerAirBase;
	public float AttackPowerAirPerLvl;
	public float AttackPowerHolyBase;
	public float AttackPowerHolyPerLvl;
	public float AttackPowerDarkBase;
	public float AttackPowerDarkPerLvl;

	/// <summary>
	/// Recalculates combat stats from level and base growth values.
	/// Call this on level up or initialization.
	/// </summary>
	public void RecalculateStats(int level)
	{
		int levelBonus = Mathf.Max(0, level - 1);

		AttackPower = AttackPowerBase + levelBonus*AttackPowerPerLvl;
		Defense = DefenseBase + levelBonus*DefensePerLvl;

		AttackPowerFire = AttackPowerFireBase + levelBonus*AttackPowerFirePerLvl;
		AttackPowerWater = AttackPowerWaterBase + levelBonus*AttackPowerWaterPerLvl;
		AttackPowerEarth = AttackPowerEarthBase + levelBonus*AttackPowerEarthPerLvl;
		AttackPowerLightning = AttackPowerLightningBase + levelBonus*AttackPowerLightningPerLvl;
		AttackPowerHoly = AttackPowerHolyBase + levelBonus*AttackPowerHolyPerLvl;
		AttackPowerDark = AttackPowerDarkBase + levelBonus*AttackPowerDarkPerLvl;
	}
}
