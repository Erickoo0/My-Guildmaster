using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// API to Handle the generation of worker recruits.
/// Assigns recruits random names from a pool, and random stats based on various criteria.
/// </summary>
public class WorkerRecruitGenerator
{
	private readonly WorkerGenerationProfile _workerGenerationProfile;

	// Constructor
	public WorkerRecruitGenerator(WorkerGenerationProfile workerGenerationProfile)
	{
		_workerGenerationProfile = workerGenerationProfile;
	}

	/// <summary>
	/// Generates a pool of worker recruits.
	/// </summary>
	public List<WorkerData> GenerateRecruitPool()
	{
		// 1. Create an empty list
		List<WorkerData> recruitsList = new List<WorkerData>();

		if (_workerGenerationProfile == null)
		{
			Debug.LogWarning("WorkerRecruitGenerator: No WorkerGenerationProfile assigned.");
			return recruitsList;

		}

		// 2. Generate recruits based off the recruit pool size
		for (int i = 0; i < _workerGenerationProfile.RecruitPoolSize; i++)
			recruitsList.Add(GenerateRecruit());


		return recruitsList;
	}

	/// <summary>
	/// Generates a single worker recruit.
	/// </summary>
	public WorkerData GenerateRecruit()
	{
		if (_workerGenerationProfile == null)
		{
			Debug.LogWarning("WorkerRecruitGenerator: No WorkerGenerationProfile assigned.");
			return null;
		}

		string workerID = Guid.NewGuid().ToString(); // Assigns a unique ID to the worker
		string workerName = GetRandomName();
		Sprite workerPortrait = GetRandomPortrait();

		int skillLevel = Random.Range(_workerGenerationProfile.SkillLevelMin, _workerGenerationProfile.SkillLevelMax + 1);
		int maintenanceLevel = Random.Range(_workerGenerationProfile.MaintenanceLevelMin, _workerGenerationProfile.MaintenanceLevelMax + 1);
		int salary = Random.Range(_workerGenerationProfile.SalaryMin, _workerGenerationProfile.SalaryMax + 1);
		int sustenanceRequiredPerDay = Random.Range(_workerGenerationProfile.SustenanceRequiredPerDayMin, _workerGenerationProfile.SustenanceRequiredPerDayMax + 1);

		return new WorkerData(workerID, workerName, workerPortrait, skillLevel, maintenanceLevel, salary, sustenanceRequiredPerDay);
	}

	private string GetRandomName()
	{
		// Safety Check
		if (_workerGenerationProfile.WorkerFirstNamePool == null || _workerGenerationProfile.WorkerFirstNamePool.Length == 0 ||
			_workerGenerationProfile.WorkerLastNamePool == null || _workerGenerationProfile.WorkerLastNamePool.Length == 0)
			return "Unnamed Worker";

		// Pick a random first name and last name from the pool and add them together
		return _workerGenerationProfile.WorkerFirstNamePool[Random.Range(0, _workerGenerationProfile.WorkerFirstNamePool.Length)] + " " +
			_workerGenerationProfile.WorkerLastNamePool[Random.Range(0, _workerGenerationProfile.WorkerLastNamePool.Length)];
	}

	private Sprite GetRandomPortrait()
	{
		if (_workerGenerationProfile.WorkerPortraitPool == null || _workerGenerationProfile.WorkerPortraitPool.Length == 0)
			return null;

		return _workerGenerationProfile.WorkerPortraitPool[Random.Range(0, _workerGenerationProfile.WorkerPortraitPool.Length)];
	}
}
