using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Manages the Research System, UI.
/// Spawns ResearchNodes, draws Prerequisite connection lines, displays node details, and routes input to ResearchManager.
/// </summary>
public class ResearchUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _researchMenu;
	[SerializeField] private GameObject _researchNodePrefab;
	[SerializeField] private RectTransform _researchNodeContainer;
	[SerializeField] private Button _researchNodeButton;

	[Header("Node Details")]
	[SerializeField] private Image _researchNodeIcon;
	[SerializeField] private TextMeshProUGUI _researchNodeName;
	[SerializeField] private TextMeshProUGUI _researchNodeDescription;

	[Header("Connection Lines")]
	[SerializeField] private RectTransform _connectionLinePrefab;

	private readonly List<RectTransform> _connectionLinesList = new List<RectTransform>();
	private readonly Dictionary<string, Vector2> _researchNodePositionsList = new Dictionary<string, Vector2>();
	private readonly List<ResearchNodeUI> _researchNodeUIList = new List<ResearchNodeUI>();

	private ResearchNode _selectedResearchNode;

	private void OnEnable()
	{
		EventBus.OnResearchChanged += RefreshUI;
		EventBus.OnMenuToggleRequested += OnMenuToggleRequested;
		_researchNodeButton.onClick.AddListener(OnResearchButtonClicked);
	}

	private void OnDisable()
	{
		EventBus.OnResearchChanged -= RefreshUI;
		EventBus.OnMenuToggleRequested -= OnMenuToggleRequested;

		ClearUI();
	}

	#region MENU

	private void OnMenuToggleRequested(MenuType menuType)
	{
		if (menuType != MenuType.Research)
			return;

		if (!_researchMenu.activeSelf)
			OpenMenu();
		else
		{
			CloseMenu();
		}
	}

	private void OpenMenu()
	{
		EventBus.RequestOpenMenu(_researchMenu);
		BuildResearchTree();
	}

	private void CloseMenu()
	{
		EventBus.RequestCloseMenu(_researchMenu);
	}

	#endregion

	#region UI_CONSTRUCTION

	/// <summary>
	/// Generate the visual nodes, draws connection lines, and updates text
	/// </summary>
	private void BuildResearchTree()
	{
		ClearUI();

		// 1. Get the Research Tree
		ResearchTree researchTree = ResearchManager.Instance.ResearchTree;

		// 2. Build the ResearchNodes
		foreach (ResearchNode researchNode in researchTree.ResearchNodesList)
		{
			if (researchNode == null)
				continue;

			// 3 Instantiate and position
			GameObject researchNodePrefab = Instantiate(_researchNodePrefab, _researchNodeContainer);
			RectTransform rectTransform = researchNodePrefab.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = researchNode.Position;
			_researchNodePositionsList[researchNode.ID] = rectTransform.anchoredPosition;

			// 4. Setup data and add to list
			ResearchNodeUI researchNodeUI = researchNodePrefab.GetComponent<ResearchNodeUI>();
			researchNodeUI?.Setup(researchNode, ResearchManager.Instance, OnResearchNodeSelected);
			_researchNodeUIList.Add(researchNodeUI);
		}

		// 5. Build connections & update state
		DrawPrerequisiteLines();
		RefreshUI();
	}

	private void DrawPrerequisiteLines()
	{
		foreach (ResearchNode researchNode in ResearchManager.Instance.ResearchTree.ResearchNodesList)
		{
			if (researchNode?.PrerequisitesList == null)
				continue;

			// 1. Loop through all prerequisites
			foreach (ResearchNodePrerequisite prerequisite in researchNode.PrerequisitesList)
			{
				// 2. Verify that current node and its prerequisites have been created and positioned
				if (prerequisite == null
					|| !_researchNodePositionsList.TryGetValue(researchNode.ID, out Vector2 toPosition)
					|| !_researchNodePositionsList.TryGetValue(prerequisite.RequiredResearchNodeID, out Vector2 fromPosition))
					continue;

				// 3. Spawn and orient connection lines between ResearchNodes
				RectTransform connectionLine = Instantiate(_connectionLinePrefab, _researchNodeContainer);
				connectionLine.SetAsFirstSibling(); // Render behind other UI elements
				Vector2 delta = toPosition - fromPosition;
				connectionLine.sizeDelta = new Vector2(delta.magnitude, connectionLine.sizeDelta.y);
				connectionLine.anchoredPosition = fromPosition;
				connectionLine.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x)*Mathf.Rad2Deg);

				_connectionLinesList.Add(connectionLine);
			}
		}
	}

	private void RefreshUI()
	{
		// 1. Loop through all instantiated ResearchNodeUIs and refresh them.
		foreach (ResearchNodeUI researchNodeUI in _researchNodeUIList)
			researchNodeUI.RefreshUI();

		// 2. Refresh selected research node details
		if (_selectedResearchNode == null)
		{
			_researchNodeIcon.sprite = null;
			_researchNodeIcon.enabled = false;
			_researchNodeName.text = "Select Research";
			_researchNodeDescription.text = "";
			_researchNodeButton.interactable = false;
			return;
		} else
		{
			_researchNodeIcon.sprite = _selectedResearchNode.Icon;
			_researchNodeIcon.enabled = true;
			_researchNodeName.text = _selectedResearchNode.DisplayName;
			_researchNodeDescription.text = _selectedResearchNode.Description;
			_researchNodeButton.interactable = ResearchManager.Instance.CanUnlockResearchNode(_selectedResearchNode.ID);
		}
	}

	private void ClearUI()
	{
		// 1. Loop through all instantiated ResearchNodeUIs and destroy them
		foreach (ResearchNodeUI researchNodeUI in _researchNodeUIList)
			Destroy(researchNodeUI?.gameObject);

		// 2. Loop through all instantiated ConnectionLines and destroy them
		foreach (RectTransform connectionLine in _connectionLinesList)
			Destroy(connectionLine?.gameObject);

		// 3. Clear the list
		_researchNodeUIList.Clear();
		_researchNodePositionsList.Clear();
		_connectionLinesList.Clear();
		_selectedResearchNode = null;
		RefreshUI();
	}

	#endregion

	#region PLAYER_INPUT

	private void OnResearchNodeSelected(ResearchNode researchNode)
	{
		_selectedResearchNode = researchNode;
		RefreshUI();
	}

	private void OnResearchButtonClicked()
	{
		if (_selectedResearchNode == null)
			return;

		bool changed = ResearchManager.Instance.TryUnlockResearchNode(_selectedResearchNode.ID);

		if (changed)
			RefreshUI();
	}

	#endregion
}
