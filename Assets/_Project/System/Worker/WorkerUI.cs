using TMPro;
using UnityEngine;
/// <summary>
/// Updates HUD elements for the WorkerManager and Worker menu.
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
