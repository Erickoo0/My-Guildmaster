using UnityEngine;
/// <summary>
/// Defines the profile for generating a recruit worker.
/// Stats, name pool, refresh rules.
/// </summary>
[CreateAssetMenu(fileName = "Worker_Generation_Profile", menuName = "Worker/Generation Profile")]
public class WorkerGenerationProfile : ScriptableObject
{
	[Header("Recruitment")]
	[field: SerializeField] public int RecruitPoolSize { get; private set; } = 3;

	[Header("Name Pool")]
	[field: SerializeField] public string[] WorkerFirstNamePool { get; private set; }
	[field: SerializeField] public string[] WorkerLastNamePool { get; private set; }

	[Header("Visuals")]
	[field: SerializeField] public Sprite[] WorkerPortraitPool { get; private set; }

	[Header("Skill Level")]
	[field: SerializeField] public int SkillLevelMin { get; private set; } = 1;
	[field: SerializeField] public int SkillLevelMax { get; private set; } = 5;

	[Header("Maintenance Level")]
	[field: SerializeField] public int MaintenanceLevelMin { get; private set; } = 1;
	[field: SerializeField] public int MaintenanceLevelMax { get; private set; } = 5;

	[Header("Salary")]
	[field: SerializeField] public int SalaryMin { get; private set; } = 5;
	[field: SerializeField] public int SalaryMax { get; private set; } = 20;

	[Header("Upkeep")]
	[field: SerializeField] public int SustenanceRequiredPerDayMin { get; private set; } = 1;
	[field: SerializeField] public int SustenanceRequiredPerDayMax { get; private set; } = 3;

	private void OnValidate()
	{
		RecruitPoolSize = Mathf.Max(0, RecruitPoolSize);

		SkillLevelMin = Mathf.Max(1, SkillLevelMin);
		SkillLevelMax = Mathf.Max(SkillLevelMin, SkillLevelMax);

		MaintenanceLevelMin = Mathf.Max(1, MaintenanceLevelMin);
		MaintenanceLevelMax = Mathf.Max(MaintenanceLevelMin, MaintenanceLevelMax);

		SalaryMin = Mathf.Max(0, SalaryMin);
		SalaryMax = Mathf.Max(SalaryMin, SalaryMax);

		SustenanceRequiredPerDayMin = Mathf.Max(0, SustenanceRequiredPerDayMin);
		SustenanceRequiredPerDayMax = Mathf.Max(SustenanceRequiredPerDayMin, SustenanceRequiredPerDayMax);
	}
}
