using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// View + ControllerEntity for a single SkillNode button in the SKillTreeUI
/// Displays SkillNode state and routes click events to the SkillTree blueprint.
/// </summary>
public class SkillNodeUI : MonoBehaviour
{
	public enum NodeState { Locked, Available, Allocated, Maxed }

	[Header("References")]
	[SerializeField] private Button _button;
	[SerializeField] private Image _icon;
	[SerializeField] private Image _background;
	[SerializeField] private TextMeshProUGUI _rankText;
	[SerializeField] private TextMeshProUGUI _nameText;

	[Header("State Colors")]
	[SerializeField] private Color _colorLocked = new Color(0.45f, 0.45f, 0.45f, 0.4f);
	[SerializeField] private Color _colorAvailable = new Color(0.85f, 0.8f, 0.4f, 0.6f);
	[SerializeField] private Color _colorAllocated = new Color(0.45f, 0.75f, 0.5f, 0.75f);
	[SerializeField] private Color _colorMaxed = new Color(0.45f, 0.65f, 0.85f, 0.85f);

	private Action _onSkillNodeChanged;

	private SkillNode _skillNode;
	private SkillTree _skillTree;
	private SkillTreeLedger _skillTreeLedger;

	public bool HasTooltipData => _skillNode != null;
	public string TooltipTitle => _skillNode != null ? _skillNode.DisplayName : "Unknown SkillNode";
	public string TooltipDescription
	{
		get
		{
			if (_skillNode == null)
				return "";

			string baseDescription = _skillNode.Description;
			int allocatedPoints = _skillTreeLedger.GetAllocatedSkillPoints(_skillNode.ID);
			return $"{baseDescription}\n\n<color=#a0a0a0>Rank: {allocatedPoints} / {_skillNode.SkillPointsMax}</color>";
		}
	}

	public void Setup(SkillNode skillNode, SkillTree skillTree, SkillTreeLedger skillTreeLedger, Action onSkillNodeChanged)
	{
		_skillNode = skillNode;
		_skillTree = skillTree;
		_skillTreeLedger = skillTreeLedger;
		_onSkillNodeChanged = onSkillNodeChanged;

		// Wire up the button click
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(OnAllocateRequested);

		// Update display
		_icon.sprite = skillNode.Icon;
		_icon.enabled = skillNode.Icon != null;

		name = $"skillNodeUI_{skillNode.ID}";

		RefreshUI();
	}

	public void RefreshUI()
	{
		if (_skillNode == null || _skillTreeLedger == null)
			return;

		// 1. Apply state visuals
		NodeState nodeState = GetCurrentState();
		ApplyStateVisuals(nodeState);

		int allocated = _skillTreeLedger.GetAllocatedSkillPoints(_skillNode.ID);
		_rankText.text = _skillNode.SkillPointsMax > 1
			? $"{allocated}/{_skillNode.SkillPointsMax}"
			: (allocated > 0 ? "✓" : "");

		_nameText.text = _skillNode.DisplayName;
	}

	//----HELPER METHODS----

	private NodeState GetCurrentState()
	{
		int allocated = _skillTreeLedger.GetAllocatedSkillPoints(_skillNode.ID);

		if (allocated >= _skillNode.SkillPointsMax)
			return NodeState.Maxed;
		else if (allocated > 0)
			return NodeState.Allocated;
		else if (_skillTree.CanAllocateSkillPoint(_skillNode.ID, _skillTreeLedger))
			return NodeState.Available;
		else
			return NodeState.Locked;
	}

	private void ApplyStateVisuals(NodeState nodeState)
	{
		switch (nodeState)
		{
		case NodeState.Maxed:
			_background.color = _colorMaxed;
			_icon.color = Color.white;
			break;
		case NodeState.Allocated:
			_background.color = _colorAllocated;
			_icon.color = Color.white;
			break;
		case NodeState.Available:
			_background.color = _colorAvailable;
			_icon.color = Color.white;
			break;
		case NodeState.Locked:
			_background.color = _colorLocked;
			// Soften the icon slightly for locked nodes as well
			_icon.color = new Color(1f, 1f, 1f, 0.4f);
			break;
		}
	}

	//----INPUT METHODS----

	private void OnAllocateRequested()
	{
		// Safety Check
		if (_skillTree == null || _skillTreeLedger == null) return;

		bool changed = _skillTree.TryAllocateSkillPoint(_skillNode.ID, _skillTreeLedger);
		if (changed)
			_onSkillNodeChanged?.Invoke();
	}

	public void OnRefundRequested()
	{
		// Safety Check
		if (_skillTree == null || _skillTreeLedger == null) return;

		bool changed = _skillTree.TryRefundSkillPoint(_skillNode.ID, _skillTreeLedger);

		if (changed)
			_onSkillNodeChanged?.Invoke();
	}
}
