using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Identifies the type of assignment a worker is currently performing.
/// Links to stations.
/// </summary>
public enum WorkerAssignmentType
{
	None,
	Alchemy,
	Crafting,
	Research,
	Cooking,
	TrainingRoom,
	Mine,
	Lumbermill,
	Farm,
	DungeonTeam,
	SkillResearch
}

/// <summary>
/// Track the workers current condition
/// </summary>
public enum WorkerStatus
{
	Available,
	Assigned,
	Unavailable,
	Hungry,
	Unpaid
}

/// <summary>
/// Manages the worker system.
/// Handles recruits, worker management, assigning workers to stations, managing salary and sustenance.
/// </summary>
public class WorkerManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private WorkerGenerationProfile _workerGenerationProfile;
	private readonly List<WorkerData> _hiredWorkersList = new List<WorkerData>();
	private readonly List<WorkerData> _recruitPoolList = new List<WorkerData>();

	private WorkerRecruitGenerator _workerRecruitGenerator;
	public static WorkerManager Instance { get; private set; }

	public int CurrentSustenance { get; private set; }
	public IReadOnlyList<WorkerData> HiredWorkersList => _hiredWorkersList;
	public IReadOnlyList<WorkerData> RecruitPoolList => _recruitPoolList;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;

		// Create a worker recruit generator passing the workerGenerationProfile scriptableObject
		_workerRecruitGenerator = new WorkerRecruitGenerator(_workerGenerationProfile);
	}

	private void Start()
	{
		// Subscribe to all events
		EventBus.OnDayChanged += HandleDayChanged;
		ItemStoragePlayer.Instance.OnSlotUpdated += HandleInventoryChanged;
		EventBus.OnWorkerHireRequested += HireWorker;
		EventBus.OnWorkerReleaseRequested += ReleaseWorker;
		EventBus.OnWorkerAssignRequested += AssignWorker;

		RefreshRecruitPool();
		RecalculateSustenance();
	}

	private void OnDestroy()
	{
		// Unsubscribe from all events
		EventBus.OnDayChanged -= HandleDayChanged;
		ItemStoragePlayer.Instance.OnSlotUpdated -= HandleInventoryChanged;
		EventBus.OnWorkerHireRequested -= HireWorker;
		EventBus.OnWorkerReleaseRequested -= ReleaseWorker;
		EventBus.OnWorkerAssignRequested -= AssignWorker;
	}

	#region SUSTENANCE_AND_UPKEEP

	private void HandleInventoryChanged(int slotIndex) => RecalculateSustenance();

	private void HandleDayChanged(object sender, TimeSpan time) => PayDailyUpkeep();

	/// <summary>
	/// Calculates the total sustenance value of all items in the player's inventory.
	/// </summary>
	private void RecalculateSustenance()
	{
		int newSustenance = 0;

		for (int i = 0; i < ItemStoragePlayer.Instance.StorageCapacity; i++)
		{
			ItemInstance item = ItemStoragePlayer.Instance.GetItem(i);

			if (item == null || item.DataSo == null)
				continue;

			// Check for any items with a food property
			if (item.DataSo.TryGetProperty(out ItemPropertyFood foodProperty))
				newSustenance += foodProperty.SustenanceValue*item.stackSize;
		}

		// If value has changed, broadcast it
		if (newSustenance != CurrentSustenance)
		{
			CurrentSustenance = newSustenance;
			EventBus.RequestTotalSustenanceChanged(CurrentSustenance);
		}
	}

	/// <summary>
	/// Calculate the total daily salary of all hired workers
	/// </summary>
	/// <returns></returns>
	public int GetTotalDailySalary()
	{
		int totalSalary = 0;

		// 1. Loop through all hired workers and add their salaries
		foreach (WorkerData workerData in _hiredWorkersList)
			totalSalary += workerData.Salary;

		return totalSalary;
	}

	/// <summary>
	/// Calculate the total daily sustenance required of all hired workers
	/// </summary>
	/// <returns></returns>
	public int GetTotalDailySustenance()
	{
		int totalSustenance = 0;

		foreach (WorkerData workerData in _hiredWorkersList)
			totalSustenance += workerData.SustenanceRequiredPerDay;

		return totalSustenance;
	}

	private void PayDailyUpkeep()
	{
		// 1. Check if we actually have workers
		if (_hiredWorkersList.Count == 0)
			return;

		int totalSustenanceRequired = GetTotalDailySustenance();

		// 2. If we have enough sustenance, feed the workers!
		if (CurrentSustenance >= totalSustenanceRequired)
		{
			foreach (WorkerData workerData in _hiredWorkersList)
			{
				// If the worker was previously hungry, update their status as they have been fed!
				if (workerData.Status == WorkerStatus.Hungry)
				{
					WorkerStatus newStatus = workerData.AssignmentType == WorkerAssignmentType.None ? WorkerStatus.Available : WorkerStatus.Assigned;
					workerData.SetStatus(newStatus);
				}
			}
		}
		// 3. If we don't have enough sustenance, set workers to Hungry
		else
		{
			foreach (WorkerData workerData in _hiredWorkersList)
				workerData.SetStatus(WorkerStatus.Hungry);
		}

		// 4. Consume FOod Logic implement later

		// 5. Check if we have enough Gold

		// 6. Set worker status

		// 7. Pay Gold

		EventBus.RequestWorkerDataChanged();
	}

	#endregion

	#region ASSIGNMENT_AND_STATION

	/// <summary>
	/// Refreshes the recruit pool list with new workers generated by the worker recruit generator.
	/// </summary>
	public void RefreshRecruitPool()
	{
		_recruitPoolList.Clear();

		if (_workerGenerationProfile == null)
		{
			Debug.LogWarning("WorkerManageR: WorkerRecruitGenerator is null.");
			EventBus.RequestWorkerDataChanged();
			return;
		}

		// 1. Tell the Recruit API to generate a new pool of recruits and add it to the list
		List<WorkerData> generatedRecruitsList = _workerRecruitGenerator.GenerateRecruitPool();
		foreach (WorkerData workerData in generatedRecruitsList)
		{
			if (workerData != null)
				_recruitPoolList.Add(workerData);
		}

		EventBus.RequestWorkerDataChanged();
	}

	public void HireWorker(WorkerData workerData)
	{
		// Safety Check
		if (workerData == null || !_recruitPoolList.Contains(workerData))
			return;

		// 1. Remove the recruit from the recruit pool list
		_recruitPoolList.Remove(workerData);

		// 2. Add the worker to the hired workers list
		_hiredWorkersList.Add(workerData);

		EventBus.RequestWorkerDataChanged();
	}

	public void ReleaseWorker(WorkerData workerData)
	{
		// Safety Check
		if (workerData == null || !_hiredWorkersList.Contains(workerData))
			return;

		// 1. Remove the worker from the hired workers list
		_hiredWorkersList.Remove(workerData);

		EventBus.RequestWorkerDataChanged();
	}

	public void AssignWorker(WorkerData workerData, WorkerAssignmentType assignmentType)
	{
		if (workerData == null || !_hiredWorkersList.Contains(workerData))
			return;

		workerData.AssignWorker(assignmentType);

		EventBus.RequestWorkerDataChanged();
	}

	/// <summary>
	/// Get a list of workers assigned to a given specific station
	/// </summary>
	public List<WorkerData> GetWorkersAssignedToStation(WorkerAssignmentType assignmentType)
	{
		// 1. Create a list for the workers assigned to the station
		List<WorkerData> assignedWorkersList = new List<WorkerData>();

		foreach (WorkerData workerData in _hiredWorkersList)
		{
			if (workerData.AssignmentType == assignmentType)
				assignedWorkersList.Add(workerData);
		}
		return assignedWorkersList;
	}

	public float GetWorkerWorkMultiplier(WorkerAssignmentType assignmentType)
	{
		// 1. Get the list of assigned workers from the given station
		List<WorkerData> assignedWorkersList = GetWorkersAssignedToStation(assignmentType);

		// 2. If there are no workers, just return 
		if (assignedWorkersList.Count == 0)
			return 1f;

		float totalMultiplier = 1f;

		// 3. Iterate through the list of assigned workers and calculate the total multiplier
		foreach (WorkerData workerData in assignedWorkersList)
			totalMultiplier += workerData.GetWorkMultiplier() - 1f;

		return totalMultiplier;
	}

	#endregion
}
