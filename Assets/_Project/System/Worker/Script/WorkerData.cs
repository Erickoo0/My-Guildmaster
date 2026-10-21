using System;
using UnityEngine;
/// <summary>
/// Runtime data class for an individual worker.
/// Handles metadata, assignment, and live stats.
/// </summary>
[Serializable]
public class WorkerData
{
	[Header("Identity")]
	[field: SerializeField] public string WorkerID { get; private set; }
	[field: SerializeField] public string WorkerName { get; private set; }
	[field: SerializeField] public Sprite WorkerPortrait { get; private set; }

	[Header("Stats")]
	[field: SerializeField] public int SkillLevel { get; private set; }
	[field: SerializeField] public int MaintenanceLevel { get; private set; }
	[field: SerializeField] public int Salary { get; private set; }
	[field: SerializeField] public int SustenanceRequiredPerDay { get; private set; }

	[Header("State")]
	[field: SerializeField] public WorkerAssignmentType AssignmentType { get; private set; }
	[field: SerializeField] public WorkerStatus Status { get; private set; }

	// Constructor
	public WorkerData(
		string workerID, string workerName, Sprite workerPortrait,
		int skillLevel, int maintenanceLevel, int salary, int sustenanceRequiredPerDay
	)
	{
		// Pass all the data
		WorkerID = workerID;
		WorkerName = workerName;
		WorkerPortrait = workerPortrait;
		SkillLevel = skillLevel;
		MaintenanceLevel = maintenanceLevel;
		Salary = salary;
		SustenanceRequiredPerDay = sustenanceRequiredPerDay;

		AssignmentType = WorkerAssignmentType.None;
		Status = WorkerStatus.Available;
	}

	public void AssignWorker(WorkerAssignmentType assignmentType)
	{
		// 1. Update the assignment type
		AssignmentType = assignmentType;

		// 2. Do not allow assignment changes to clear negaive or blocking statuses
		RefreshStatusFromAssignment();

	}

	public void SetStatus(WorkerStatus status) => Status = status;

	public void RefreshStatusFromAssignment()
	{
		if (Status == WorkerStatus.Hungry || Status == WorkerStatus.Unpaid || Status == WorkerStatus.Unavailable)
			return;

		Status = AssignmentType == WorkerAssignmentType.None ? WorkerStatus.Available : WorkerStatus.Assigned;
	}

	public float GetWorkMultiplier()
	{
		// Calculate the skill bonus based off the workers skill level
		float skillBonus = SkillLevel*0.1f;
		// Apply a penality if the worker is hungry or unpaid
		float statusPenalty = Status == WorkerStatus.Hungry || Status == WorkerStatus.Unpaid ? 0.5f : 1f;

		// Return the work speed multiplier
		return Mathf.Max(0.1f, (1f + skillBonus)*statusPenalty);
	}
}
