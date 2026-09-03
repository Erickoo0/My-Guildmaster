using TMPro;
using UnityEngine;
/// <summary>
/// Handles UI elements for the Worker System and Worker Menu
/// </summary>
public class WorkerUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI workerSustenanceText;

	private void Start()
	{
		EventBus.OnTotalSustenanceChanged += UpdateSustenanceDisplay;

		if (WorkerManager.Instance != null)
			UpdateSustenanceDisplay(WorkerManager.Instance.CurrentSustenance);
	}

	private void OnDestroy() => EventBus.OnTotalSustenanceChanged -= UpdateSustenanceDisplay;

	private void UpdateSustenanceDisplay(int totalSustenance) => workerSustenanceText.text = $"{totalSustenance}";
}
