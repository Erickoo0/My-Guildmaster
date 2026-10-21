using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles the UI for a recruit worker slot
/// </summary>
public class WorkerRecruitSlotUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image _workerPortrait;
	[SerializeField] private TextMeshProUGUI _workerName;
	[SerializeField] private TextMeshProUGUI _workerSkillLevel;
	[SerializeField] private TextMeshProUGUI _workerMaintenanceLevel;
	[SerializeField] private TextMeshProUGUI _salary;
	[SerializeField] private TextMeshProUGUI _sustenanceRequired;
	[SerializeField] private Button _slotButton;

	private WorkerData _workerData;

	private void OnDestroy() => _slotButton.onClick.RemoveAllListeners();

	public void Setup(WorkerData workerData, Action<WorkerData> OnSlotSelected)
	{
		_workerData = workerData;
		_workerPortrait.sprite = workerData.WorkerPortrait;
		_workerName.text = workerData.WorkerName;
		_workerSkillLevel.text = $"Skill Level: {workerData.SkillLevel}";
		_workerMaintenanceLevel.text = $"Maintenance Level: {workerData.MaintenanceLevel}";
		_salary.text = $"Salary: {workerData.Salary}";
		_sustenanceRequired.text = $"Sustenance Rate: {workerData.SustenanceRequiredPerDay}";
		_slotButton.onClick.RemoveAllListeners();
		_slotButton.onClick.AddListener(() => OnSlotSelected?.Invoke(workerData));
	}
}
