using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Orchestrates the Skill Tree UI panel.
/// Dynamically reads the player's active spell loadout, cycles through valid skill trees,
/// spawns interactive nodes, and draws prerequisite connection lines.
/// </summary>
public class SkillTreeUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _skillTreePanel;
	[SerializeField] private RectTransform _skillNodeContainer;
	[SerializeField] private GameObject _skillNodeUIPrefab;
	[SerializeField] private TextMeshProUGUI _skillNameText;
	[SerializeField] private TextMeshProUGUI _totalSkillPointsText;
	[SerializeField] private Image _skillIcon;

	[Header("Loadout Navigation")]
	[Tooltip("Reference to the player skill controller to read active slots.")]
	[SerializeField] private SkillControllerPlayer _playerSkillController;
	[SerializeField] private Button _nextButton;
	[SerializeField] private Button _prevButton;

	[Header("Connection Lines")]
	[Tooltip("Prefab with a UI Image used as a line segment between nodes.")]
	[SerializeField] private RectTransform _connectionLinePrefab;

	[Header("Layout")]
	[SerializeField] private float _positionScale = 1f;

	// UI Caches
	private readonly List<RectTransform> _connectionLinesList = new List<RectTransform>();
	private readonly Dictionary<string, Vector2> _nodePositions = new Dictionary<string, Vector2>();
	private readonly List<SkillNodeUI> _skillNodesUIList = new List<SkillNodeUI>();

	// Runtime State
	private SkillTree _currentSkillTree;
	private int _currentSlotIndex = 0;
	private SkillTreeLedger _skillTreeLedger;

	#region Unity Lifecycle

	private void Start()
	{
		// 1. Setup Navigation (1 = Next, -1 = Previous)
		if (_nextButton != null) _nextButton.onClick.AddListener(() => CycleSkillSlot(1));
		if (_prevButton != null) _prevButton.onClick.AddListener(() => CycleSkillSlot(-1));

		// 2. Auto-bind player controller if forgotten in the Inspector
		if (_playerSkillController == null)
			_playerSkillController = FindFirstObjectByType<SkillControllerPlayer>();

		if (_playerSkillController == null)
			Debug.LogWarning($"{name}: No SkillControllerPlayer assigned or found in scene.");
	}

	private void OnDisable() => ClearAllUI();

	#endregion

	#region Menu & Navigation

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (!_skillTreePanel.activeSelf)
		{
			EventBus.RequestOpenMenu(_skillTreePanel);

			// Smart-seek: Start on the first valid skill that actually has a skill tree blueprint
			_currentSlotIndex = GetFirstValidSlotIndex();
			LoadAndBuildSkillTree(_currentSlotIndex);
		} else
		{
			EventBus.RequestCloseMenu(_skillTreePanel);
		}
	}

	/// <summary>
	/// Cycles the UI left (-1) or right (1) through the player's active loadout,
	/// automatically skipping empty slots or skills that do not have a Skill Tree.
	/// </summary>
	private void CycleSkillSlot(int direction)
	{
		if (_playerSkillController == null || _playerSkillController.SkillSlots.Count == 0) return;

		int maxSlots = _playerSkillController.SkillSlots.Count;

		// Iterate through slots to find the next valid one
		for (int i = 1; i <= maxSlots; i++)
		{
			// Calculate wrapping index based on direction
			int checkIndex = (_currentSlotIndex + (i*direction) + maxSlots)%maxSlots;
			var checkSpell = _playerSkillController.SkillSlots[checkIndex];

			// Only stop if the slot contains a spell AND that spell has a tree blueprint
			if (checkSpell != null && checkSpell.SkillTreeInstance != null)
			{
				_currentSlotIndex = checkIndex;
				LoadAndBuildSkillTree(_currentSlotIndex);
				return;
			}
		}
	}

	private int GetFirstValidSlotIndex()
	{
		if (_playerSkillController == null || _playerSkillController.SkillSlots == null) return 0;

		for (int i = 0; i < _playerSkillController.SkillSlots.Count; i++)
		{
			var spell = _playerSkillController.SkillSlots[i];
			if (spell != null && spell.SkillTreeInstance != null) return i;
		}
		return 0;
	}

	#endregion

	#region UI Construction

	/// <summary>
	/// Grabs data from the requested loadout index, generates the visual nodes,
	/// draws connection lines, and updates text layouts.
	/// </summary>
	private void LoadAndBuildSkillTree(int slotIndex)
	{
		ClearAllUI();

		// 1. Safety Checks
		if (_playerSkillController == null || _playerSkillController.SkillSlots.Count <= slotIndex) return;

		PlayerSkillStateBase PlayerSkillStateActive = _playerSkillController.SkillSlots[slotIndex];

		// 2. Validate Skill & Tree existence
		if (PlayerSkillStateActive == null || PlayerSkillStateActive.SkillTreeInstance == null)
		{
			_currentSkillTree = null;
			_skillTreeLedger = null;
			if (_skillNameText != null)
				_skillNameText.text = PlayerSkillStateActive == null ? "Empty Slot" : $"{PlayerSkillStateActive.SkillDataInstance?.Name ?? "Unknown"} (No Tree)";
			return;
		}

		// 3. Cache valid tree & ledger data
		_currentSkillTree = PlayerSkillStateActive.SkillTreeInstance;
		_skillTreeLedger = SkillTreeCompiler.GetOrCreateSkillTreeLedger(_currentSkillTree);

		if (_skillNameText != null)
			_skillNameText.text = _currentSkillTree.SkillData != null ? _currentSkillTree.SkillData.Name : _currentSkillTree.name;

		if (_skillIcon != null)
			_skillIcon.sprite = _currentSkillTree.SkillData != null ? _currentSkillTree.SkillData.Icon : null;

		// 4. Build Nodes
		foreach (SkillNode node in _currentSkillTree.SkillNodes)
		{
			if (node == null || _skillNodeUIPrefab == null) continue;

			// Instantiate and position
			GameObject go = Instantiate(_skillNodeUIPrefab, _skillNodeContainer);
			RectTransform rt = go.GetComponent<RectTransform>();

			Vector2 nodePos = node.UIPosition*_positionScale;
			rt.anchoredPosition = nodePos;
			_nodePositions[node.ID] = nodePos;

			// Setup data
			SkillNodeUI nodeUI = go.GetComponent<SkillNodeUI>();
			nodeUI?.Setup(node, _currentSkillTree, _skillTreeLedger, RefreshAllNodesUI);
			_skillNodesUIList.Add(nodeUI);
		}

		// 5. Build Connections & Update state
		DrawPrerequisiteConnections();
		RefreshAllNodesUI();
	}

	private void DrawPrerequisiteConnections()
	{
		if (_connectionLinePrefab == null || _currentSkillTree == null) return;

		foreach (SkillNode node in _currentSkillTree.SkillNodes)
		{
			if (node?.Prerequisites == null) continue;

			foreach (SkillNodePrerequisite prereq in node.Prerequisites)
			{
				// Verify both the current node and its prerequisite have mapped positions
				if (prereq == null ||
					!_nodePositions.TryGetValue(node.ID, out Vector2 toPos) ||
					!_nodePositions.TryGetValue(prereq.RequiredSkillNodeID, out Vector2 fromPos))
					continue;

				// Spawn and orient the connection line between the two positions
				RectTransform line = Instantiate(_connectionLinePrefab, _skillNodeContainer);
				line.SetAsFirstSibling(); // Render behind the node buttons

				Vector2 delta = toPos - fromPos;
				line.sizeDelta = new Vector2(delta.magnitude, line.sizeDelta.y);
				line.anchoredPosition = fromPos;
				line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x)*Mathf.Rad2Deg);

				_connectionLinesList.Add(line);
			}
		}
	}

	/// <summary>
	/// Forces all active node buttons to recalculate their Locked/Available/Maxed state,
	/// and updates the total points spent counter.
	/// </summary>
	private void RefreshAllNodesUI()
	{
		foreach (SkillNodeUI nodeUI in _skillNodesUIList)
			nodeUI.RefreshUI();

		if (_totalSkillPointsText != null && _skillTreeLedger != null)
			_totalSkillPointsText.text = $"Points Spent: {_skillTreeLedger.GetTotalAllocatedSkillPoints()}";
	}

	private void ClearAllUI()
	{
		foreach (SkillNodeUI nodeUI in _skillNodesUIList) Destroy(nodeUI?.gameObject);
		foreach (RectTransform line in _connectionLinesList) Destroy(line?.gameObject);

		_skillNodesUIList.Clear();
		_connectionLinesList.Clear();
		_nodePositions.Clear();
	}

	#endregion
}
