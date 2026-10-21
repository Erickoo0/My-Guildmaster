using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles the UI for a hired worker slot.
/// </summary>
public class WorkerDataSlotUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image _workerPortrait;
	[SerializeField] private TextMeshProUGUI _workerName;
	[SerializeField] private TextMeshProUGUI _workerSkillLevel;
	[SerializeField] private TextMeshProUGUI _workerMaintenanceLevel;
	[SerializeField] private TextMeshProUGUI _salary;
	[SerializeField] private TextMeshProUGUI _sustenanceRequired;
	[SerializeField] private TextMeshProUGUI _assignment;
	[SerializeField] private TextMeshProUGUI _status;
	[SerializeField] private Button _unassignButton;
	[SerializeField] private Button _releaseButton;

	private WorkerData _workerData;

	private void OnDestroy()
	{
		_unassignButton.onClick.RemoveAllListeners();
		_releaseButton.onClick.RemoveAllListeners();
	}

	public void Setup(WorkerData workerData)
	{
		_workerData = workerData;
		_workerPortrait.sprite = workerData.WorkerPortrait;
		_workerName.text = workerData.WorkerName;
		_workerSkillLevel.text = $"Skill Level: {workerData.SkillLevel}";
		_workerMaintenanceLevel.text = $"Maintenance Level: {workerData.MaintenanceLevel}";
		_salary.text = $"Salary: {workerData.Salary}";
		_sustenanceRequired.text = $"Sustenance Rate: {workerData.SustenanceRequiredPerDay}";
		_assignment.text = $"Assignment: {workerData.AssignmentType}";
		_status.text = $"Status: {workerData.Status}";

		_unassignButton.onClick.RemoveAllListeners();
		_unassignButton.onClick.AddListener(UnassignWorker);
		_releaseButton.onClick.RemoveAllListeners();
		_releaseButton.onClick.AddListener(ReleaseWorker);
	}

	public void AssignToAlchemy() => AssignWorker(WorkerAssignmentType.Alchemy);
	public void AssignToCrafting() => AssignWorker(WorkerAssignmentType.Crafting);
	public void AssignToResearch() => AssignWorker(WorkerAssignmentType.Research);
	public void AssignToCooking() => AssignWorker(WorkerAssignmentType.Cooking);
	public void AssignToTrainingRoom() => AssignWorker(WorkerAssignmentType.TrainingRoom);
	public void AssignToMine() => AssignWorker(WorkerAssignmentType.Mine);
	public void AssignToLumbermill() => AssignWorker(WorkerAssignmentType.Lumbermill);
	public void AssignToFarm() => AssignWorker(WorkerAssignmentType.Farm);
	public void AssignToDungeonTeam() => AssignWorker(WorkerAssignmentType.DungeonTeam);
	public void AssignToSkillResearch() => AssignWorker(WorkerAssignmentType.SkillResearch);

	private void AssignWorker(WorkerAssignmentType assignmentType) => EventBus.RequestWorkerAssign(_workerData, assignmentType);

	private void UnassignWorker() => EventBus.RequestWorkerAssign(_workerData, WorkerAssignmentType.None);

	private void ReleaseWorker() => EventBus.RequestWorkerRelease(_workerData);
}
