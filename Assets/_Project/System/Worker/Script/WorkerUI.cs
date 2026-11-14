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
	[SerializeField] private TextMeshProUGUI _header;

	[Header("Tabs")]
	[SerializeField] private Button _recruitTabButton;
	[SerializeField] private Button _workerTabButton;
	[SerializeField] private GameObject _recruitPanel;
	[SerializeField] private GameObject _workerPanel;

	[Header("Worker Lists")]
	[SerializeField] private WorkerRecruitSlotUI _recruitSlotPrefab;
	[SerializeField] private Transform _recruitSlotContainer;
	[SerializeField] private WorkerDataSlotUI _workerSlotPrefab;
	[SerializeField] private Transform _workerSlotContainer;

	[Header("Guild Information")]
	[SerializeField] private TextMeshProUGUI totalSustenanceText;
	[SerializeField] private TextMeshProUGUI _dailySustenanceText;
	[SerializeField] private TextMeshProUGUI _dailySalaryText;

	[Header("Recruits Panel")]
	[SerializeField] private GameObject _recruitDetailsPanel;
	[SerializeField] private Image _recruitPortrait;
	[SerializeField] private TextMeshProUGUI _recruitName;
	[SerializeField] private TextMeshProUGUI _recruitSkillLevel;
	[SerializeField] private TextMeshProUGUI _recruitMaintenanceLevel;
	[SerializeField] private TextMeshProUGUI _recruitSalary;
	[SerializeField] private TextMeshProUGUI _recruitDescription;
	[SerializeField] private Button _hireButton;

	[Header("Workers Panel")]
	[SerializeField] private GameObject _workerDetailsPanel;
	[SerializeField] private Image _workerPortrait;
	[SerializeField] private TextMeshProUGUI _workerName;
	[SerializeField] private TextMeshProUGUI _workerSpecialization;
	[SerializeField] private TextMeshProUGUI _workerSkillLevel;
	[SerializeField] private TextMeshProUGUI _workerMaintenanceLevel;
	[SerializeField] private TextMeshProUGUI _workerSalary;
	[SerializeField] private TextMeshProUGUI _workerDescription;
	[SerializeField] private TextMeshProUGUI _workerAssignment;
	[SerializeField] private TextMeshProUGUI _workerStatus;
	[SerializeField] private Button _unassignButton;
	[SerializeField] private Button _releaseButton;

	private readonly MenuType _menuType = MenuType.Worker;
	private WorkerUITab _currentTab = WorkerUITab.Recruits;
	private WorkerData _selectedWorker;

	private void Start()
	{
		EventBus.OnMenuToggleRequested += HandleMenuToggle;
		EventBus.OnTotalSustenanceChanged += UpdateSustenanceDisplay;
		EventBus.OnWorkerDataChanged += RefreshUI;

		_hireButton.onClick.AddListener(HireWorker);
		_unassignButton.onClick.AddListener(UnassignWorker);
		_releaseButton.onClick.AddListener(ReleaseWorker);
		_recruitTabButton.onClick.AddListener(() => SwitchTab(WorkerUITab.Recruits));
		_workerTabButton.onClick.AddListener(() => SwitchTab(WorkerUITab.Workers));

		UpdateSustenanceDisplay(WorkerManager.Instance.CurrentSustenance);
		RefreshUI();
		SwitchTab(WorkerUITab.Recruits);
		//ClearWorkerDetails();
	}

	private void OnDestroy()
	{
		EventBus.OnMenuToggleRequested -= HandleMenuToggle;
		EventBus.OnTotalSustenanceChanged -= UpdateSustenanceDisplay;
		EventBus.OnWorkerDataChanged -= RefreshUI;

		_hireButton.onClick.RemoveListener(HireWorker);
		_unassignButton.onClick.RemoveListener(UnassignWorker);
		_releaseButton.onClick.RemoveListener(ReleaseWorker);
		_recruitTabButton.onClick.RemoveAllListeners();
		_workerTabButton.onClick.RemoveAllListeners();
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
			SwitchTab(_currentTab);

		} else if (_workerMenuPanel.activeSelf)
			EventBus.RequestCloseMenu(_workerMenuPanel);
	}

	private void SwitchTab(WorkerUITab tab)
	{
		ClearWorkerDetails();

		if (tab == WorkerUITab.Recruits)
		{
			// Swap Lists
			_recruitPanel.SetActive(true);
			_workerPanel.SetActive(false);

			// Swap Detail Panels
			_recruitDetailsPanel.SetActive(true);
			_workerDetailsPanel.SetActive(false);

			// Update Buttons & Text
			_recruitTabButton.interactable = false;
			_workerTabButton.interactable = true;
			_header.text = "Recruits";
		} else if (tab == WorkerUITab.Workers)
		{
			// Swap Lists
			_workerPanel.SetActive(true);
			_recruitPanel.SetActive(false);

			// Swap Detail Panels
			_workerDetailsPanel.SetActive(true);
			_recruitDetailsPanel.SetActive(false);

			// Update Buttons & Text
			_workerTabButton.interactable = false;
			_recruitTabButton.interactable = true;
			_header.text = "Workers";
		}

		_currentTab = tab;
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
		if (_selectedWorker != null)
			SelectWorker(_selectedWorker);
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
			recruitSlotUI.Setup(workerData, SelectWorker);
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
			workerSlotUI.Setup(workerData, SelectWorker);
		}
	}

	private void ClearSlots(Transform container)
	{
		for (int i = container.childCount - 1; i >= 0; i--)
			Destroy(container.GetChild(i).gameObject);
	}

	private void SelectWorker(WorkerData worker)
	{
		_selectedWorker = worker;

		if (_currentTab == WorkerUITab.Recruits)
		{
			_recruitPortrait.sprite = _selectedWorker.WorkerPortrait;
			_recruitPortrait.enabled = true;
			_recruitName.text = _selectedWorker.WorkerName;
			_recruitSkillLevel.text = "Skill Level: " + _selectedWorker.SkillLevel.ToString();
			_recruitMaintenanceLevel.text = "Maintenance Level: " + _selectedWorker.MaintenanceLevel.ToString();
			_recruitSalary.text = "Salary: " + _selectedWorker.Salary.ToString();
			_hireButton.interactable = true;
			_recruitDescription.text = "This is a description of the recruit.";
		} else if (_currentTab == WorkerUITab.Workers)
		{
			_workerPortrait.sprite = _selectedWorker.WorkerPortrait;
			_workerPortrait.enabled = true;
			_workerName.text = _selectedWorker.WorkerName;
			_workerSkillLevel.text = "Skill Level: " + _selectedWorker.SkillLevel.ToString();
			_workerMaintenanceLevel.text = "Maintenance Level: " + _selectedWorker.MaintenanceLevel.ToString();
			_workerSalary.text = "Salary: " + _selectedWorker.Salary.ToString();
			_workerAssignment.text = _selectedWorker.AssignmentType.ToString();
			_workerStatus.text = _selectedWorker.Status.ToString();
			_unassignButton.interactable = _selectedWorker.AssignmentType != WorkerAssignmentType.None;
			_releaseButton.interactable = true;
			_workerSpecialization.text = "";
			_workerDescription.text = "This is a description of the worker.";
		}
	}

	private void HireWorker()
	{
		if (_selectedWorker == null)
			return;

		EventBus.RequestWorkerHire(_selectedWorker);
		ClearWorkerDetails();
	}

	private void UnassignWorker()
	{
		if (_selectedWorker == null)
			return;

		EventBus.RequestWorkerAssign(_selectedWorker, WorkerAssignmentType.None);
	}

	private void ReleaseWorker()
	{
		if (_selectedWorker == null)
			return;

		EventBus.RequestWorkerRelease(_selectedWorker);
		ClearWorkerDetails();
	}

	private void ClearWorkerDetails()
	{
		_selectedWorker = null;

		// Clear Recruit Details
		_recruitPortrait.sprite = null;
		_recruitPortrait.enabled = false;
		_recruitName.text = "Select a recruit";
		_recruitSkillLevel.text = "";
		_recruitMaintenanceLevel.text = "";
		_recruitSalary.text = "";
		_hireButton.interactable = false;
		_recruitDescription.text = "";

		// Clear Worker Details
		_workerPortrait.sprite = null;
		_workerPortrait.enabled = false;
		_workerName.text = "Select a worker";
		_workerSkillLevel.text = "";
		_workerMaintenanceLevel.text = "";
		_workerSalary.text = "";
		_workerAssignment.text = "";
		_workerStatus.text = "";
		_unassignButton.interactable = false;
		_releaseButton.interactable = false;
	}

	private enum WorkerUITab { Recruits, Workers }
}
