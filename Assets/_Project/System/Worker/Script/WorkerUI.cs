using TMPro;
using UnityEngine;
/// <summary>
/// Handles UI elements for the Worker System and Worker Menu
/// </summary>
public class WorkerUI : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private GameObject _workerMenuPanel;
	[SerializeField] private TextMeshProUGUI workerSustenanceText;
	private readonly MenuType _menuType = MenuType.Worker;

	private void Start()
	{
		EventBus.OnTotalSustenanceChanged += UpdateSustenanceDisplay;

		if (WorkerManager.Instance != null)
			UpdateSustenanceDisplay(WorkerManager.Instance.CurrentSustenance);
	}

	private void OnEnable() => EventBus.OnMenuToggleRequested += HandleMenuToggle;

	private void OnDestroy()
	{
		EventBus.OnMenuToggleRequested -= HandleMenuToggle;
		EventBus.OnTotalSustenanceChanged -= UpdateSustenanceDisplay;
	}

	private void HandleMenuToggle(MenuType requestedMenu)
	{
		// 1. Check if the requested menu matches this UI menu
		if (requestedMenu != _menuType || _workerMenuPanel == null)
			return;

		// 2. Route the command to existing open menu logic
		if (!_workerMenuPanel.activeSelf)
			EventBus.RequestOpenMenu(_workerMenuPanel);
		else if (_workerMenuPanel.activeSelf)
			EventBus.RequestCloseMenu(_workerMenuPanel);
	}

	private void UpdateSustenanceDisplay(int totalSustenance) => workerSustenanceText.text = $"{totalSustenance}";
}
