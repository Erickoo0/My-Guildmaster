using System;
using UnityEngine;
/// <summary>
/// Entity stats and combat multipliers.
/// </summary>
public class EntityStats : MonoBehaviour
{

	[Header("Movement")]
	[SerializeField] private float moveSpeedMultiplier = 1f;

	[Header("Combat Multipliers")]
	[SerializeField] private float damageMultiplier = 1f;
	[SerializeField] private float damageTakenMultiplier = 1f;

	[Header("Base & Growth Values")]
	[SerializeField] private float defenseBase;
	[SerializeField] private float defensePerLvl = 1f;
	[SerializeField] private float attackPowerBase = 10f;
	[SerializeField] private float attackPowerPerLvl = 2f;

	[Header("Elemental Growth (Base / Per Level)")]
	[SerializeField] private float fireBase;
	[SerializeField] private float firePerLvl;
	[SerializeField] private float waterBase;
	[SerializeField] private float waterPerLvl;
	[SerializeField] private float earthBase;
	[SerializeField] private float earthPerLvl;
	[SerializeField] private float lightningBase;
	[SerializeField] private float lightningPerLvl;
	[SerializeField] private float airBase;
	[SerializeField] private float airPerLvl;
	[SerializeField] private float holyBase;
	[SerializeField] private float holyPerLvl;
	[SerializeField] private float darkBase;
	[SerializeField] private float darkPerLvl;

	// Runtime Calculated Properties (Read-Only to external scripts)
	public float MoveSpeedMultiplier
	{
		get => moveSpeedMultiplier;
		set => moveSpeedMultiplier = value;
	}
	public float DamageMultiplier
	{
		get => damageMultiplier;
		set => damageMultiplier = value;
	}
	public float DamageTakenMultiplier
	{
		get => damageTakenMultiplier;
		set => damageTakenMultiplier = value;
	}

	public float Defense { get; private set; }
	public float AttackPower { get; private set; }
	public float AttackPowerFire { get; private set; }
	public float AttackPowerWater { get; private set; }
	public float AttackPowerEarth { get; private set; }
	public float AttackPowerLightning { get; private set; }
	public float AttackPowerAir { get; private set; }
	public float AttackPowerHoly { get; private set; }
	public float AttackPowerDark { get; private set; }
	public event Action OnStatsRecalculated;

	/// <summary>
	/// Recalculates combat stats from level and base growth values.
	/// </summary>
	public void RecalculateStats(int level)
	{
		int levelBonus = Mathf.Max(0, level - 1);

		Defense = defenseBase + (levelBonus*defensePerLvl);
		AttackPower = attackPowerBase + (levelBonus*attackPowerPerLvl);

		AttackPowerFire = fireBase + (levelBonus*firePerLvl);
		AttackPowerWater = waterBase + (levelBonus*waterPerLvl);
		AttackPowerEarth = earthBase + (levelBonus*earthPerLvl);
		AttackPowerLightning = lightningBase + (levelBonus*lightningPerLvl);
		AttackPowerAir = airBase + (levelBonus*airPerLvl);
		AttackPowerHoly = holyBase + (levelBonus*holyPerLvl);
		AttackPowerDark = darkBase + (levelBonus*darkPerLvl);

		OnStatsRecalculated?.Invoke();
	}
}
