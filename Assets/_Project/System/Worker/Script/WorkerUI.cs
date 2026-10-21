using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles UI elements for the Worker System and Worker Menu
/// </summary>
public class WorkerUI : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private GameObject _workerMenuPanel;
	[SerializeField] private Button _hireButton;

	[Header("Worker Lists")]
	[SerializeField] private WorkerRecruitSlotUI _recruitSlotPrefab;
	[SerializeField] private Transform _recruitSlotContainer;
	[SerializeField] private WorkerDataSlotUI _workerSlotPrefab;
	[SerializeField] private Transform _workerSlotContainer;

	[Header("Guild Information")]
	[SerializeField] private TextMeshProUGUI totalSustenanceText;
	[SerializeField] private TextMeshProUGUI _dailySustenanceText;
	[SerializeField] private TextMeshProUGUI _dailySalaryText;

	[Header("Worker Information")]
	[SerializeField] private Image _workerPortrait;
	[SerializeField] private TextMeshProUGUI _workerName;
	[SerializeField] private TextMeshProUGUI _workerSpecialization;
	[SerializeField] private TextMeshProUGUI _workerSkillLevel;
	[SerializeField] private TextMeshProUGUI _workerMaintenanceLevel;
	[SerializeField] private TextMeshProUGUI _workerSalary;
	[SerializeField] private TextMeshProUGUI _workerDescription;
	private readonly MenuType _menuType = MenuType.Worker;

	private WorkerData _selectedWorker;

	private void Start()
	{
		EventBus.OnMenuToggleRequested += HandleMenuToggle;
		EventBus.OnTotalSustenanceChanged += UpdateSustenanceDisplay;
		EventBus.OnWorkerDataChanged += RefreshUI;

		_hireButton.onClick.AddListener(HireWorker);

		UpdateSustenanceDisplay(WorkerManager.Instance.CurrentSustenance);
		RefreshUI();
		ClearWorkerDetails();
	}

	private void OnDestroy()
	{
		EventBus.OnMenuToggleRequested -= HandleMenuToggle;
		EventBus.OnTotalSustenanceChanged -= UpdateSustenanceDisplay;
		EventBus.OnWorkerDataChanged -= RefreshUI;

		_hireButton.onClick.RemoveListener(HireWorker);
	}

	private void HandleMenuToggle(MenuType requestedMenu)
	{
		// 1. Check if the requested menu matches this UI menu
		if (requestedMenu != _menuType || _workerMenuPanel == null)
			return;

		// 2. Route the command to existing open menu logic
		if (!_workerMenuPanel.activeSelf)
		{
			EventBus.RequestOpenMenu(_workerMenuPanel);
			UpdateSustenanceDisplay(WorkerManager.Instance.CurrentSustenance);
			RefreshUI();

		} else if (_workerMenuPanel.activeSelf)
			EventBus.RequestCloseMenu(_workerMenuPanel);
	}

	private void UpdateSustenanceDisplay(int totalSustenance) => totalSustenanceText.text = $"{totalSustenance}";

	private void RefreshUI()
	{
		if (WorkerManager.Instance == null)
			return;

		RefreshRecruitSlots();
		RefreshHiredWorkerSlots();

		_dailySustenanceText.text = $"Daily Sustenance: {WorkerManager.Instance.GetTotalDailySustenance()}";
		_dailySalaryText.text = $"Daily Salary: {WorkerManager.Instance.GetTotalDailySalary()}";
	}

	private void RefreshRecruitSlots()
	{
		// Safety Check
		if (_recruitSlotContainer == null || _recruitSlotPrefab == null)
			return;

		// 1. Clear existing slots from container
		ClearSlots(_recruitSlotContainer);

		// 2. Create new slot prefabs
		foreach (WorkerData workerData in WorkerManager.Instance.RecruitPoolList)
		{
			WorkerRecruitSlotUI recruitSlotUI = Instantiate(_recruitSlotPrefab, _recruitSlotContainer);
			recruitSlotUI.Setup(workerData, SelectRecruit);
		}
	}

	private void RefreshHiredWorkerSlots()
	{
		// Safety Check
		if (_workerSlotContainer == null || _workerSlotPrefab == null)
			return;

		ClearSlots(_workerSlotContainer);

		foreach (WorkerData workerData in WorkerManager.Instance.HiredWorkersList)
		{
			WorkerDataSlotUI workerSlotUI = Instantiate(_workerSlotPrefab, _workerSlotContainer);
			workerSlotUI.Setup(workerData);
		}
	}

	private void ClearSlots(Transform container)
	{
		for (int i = container.childCount - 1; i >= 0; i--)
			Destroy(container.GetChild(i).gameObject);
	}

	private void SelectRecruit(WorkerData recruit)
	{
		_selectedWorker = recruit;
		_workerPortrait.sprite = _selectedWorker.WorkerPortrait;
		_workerName.text = _selectedWorker.WorkerName;
		_workerSkillLevel.text = "Skill Level: " + _selectedWorker.SkillLevel.ToString();
		_workerMaintenanceLevel.text = "Maintenance Level: " + _selectedWorker.MaintenanceLevel.ToString();
		_workerSalary.text = "Salary: " + _selectedWorker.Salary.ToString();

		if (_selectedWorker != null)
			_hireButton.interactable = true;
	}

	private void HireWorker()
	{
		if (_selectedWorker == null)
			return;

		EventBus.RequestWorkerHire(_selectedWorker);

		ClearWorkerDetails();
	}

	private void ClearWorkerDetails()
	{
		_selectedWorker = null;
		_workerPortrait.sprite = null;
		_workerName.text = "Select a recruit";
		_workerSkillLevel.text = "";
		_workerMaintenanceLevel.text = "";
		_workerSalary.text = "";

		_hireButton.interactable = false;
	}
}
