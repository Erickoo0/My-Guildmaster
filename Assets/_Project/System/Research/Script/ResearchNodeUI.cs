using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Manages the UI for a single ResearchNode in the ResearchTree.
/// </summary>
public class ResearchNodeUI : MonoBehaviour
{
	public enum NodeState { Locked, Available, Unlocked }

	[Header("References")]
	[SerializeField] private Button _button;
	[SerializeField] private Image _icon;
	[SerializeField] private TextMeshProUGUI _displayName;
	[SerializeField] private TextMeshProUGUI _description;
	[SerializeField] private Image _tint;

	[Header("State Colors")]
	[SerializeField] private Color _colorLocked = new Color(0.45f, 0.45f, 0.45f, 0.4f);
	[SerializeField] private Color _colorAvailable = new Color(0.85f, 0.8f, 0.4f, 0.6f);
	[SerializeField] private Color _colorUnlocked = new Color(0.45f, 0.75f, 0.5f, 0.85f);

	private Action<ResearchNode> _onResearchNodeSelected;
	private ResearchManager _researchManager;

	private ResearchNode _researchNode;

	// ResearchUI will call this everytime it instantiates a ResearchNodeUI
	public void Setup(ResearchNode researchNode, ResearchManager researchManager, Action<ResearchNode> onResearchNodeSelected)
	{
		_researchNode = researchNode;
		_researchManager = researchManager;
		_onResearchNodeSelected = onResearchNodeSelected;

		// Wire up the button click
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(OnResearchNodeSelected);

		// Update display
		_icon.sprite = _researchNode.Icon;
		_icon.enabled = _researchNode.Icon != null;
		_displayName.text = _researchNode.DisplayName;
		_description.text = _researchNode.Description;

		RefreshUI();
	}

	public void RefreshUI()
	{
		if (_researchNode == null || _researchManager == null)
			return;

		// 1. Apply state visuals
		NodeState nodeState = GetCurrentState();
		ApplyStateVisuals(nodeState);

		_displayName.text = _researchNode.DisplayName;
		_description.text = _researchNode.Description;
	}

	//-----HELPER METHODS------
	private NodeState GetCurrentState()
	{
		if (_researchManager.ResearchLedger != null && _researchManager.ResearchLedger.CheckResearchNodeUnlocked(_researchNode.ID))
			return NodeState.Unlocked;

		if (_researchManager.CanUnlockResearchNode(_researchNode.ID))
			return NodeState.Available;

		return NodeState.Locked;
	}

	private void ApplyStateVisuals(NodeState nodeState)
	{
		switch (nodeState)
		{
		case NodeState.Unlocked:
			_tint.color = _colorUnlocked;
			_icon.color = Color.white;
			break;
		case NodeState.Available:
			_tint.color = _colorAvailable;
			_icon.color = Color.white;
			break;
		case NodeState.Locked:
			_tint.color = _colorLocked;
			_icon.color = new Color(1f, 1f, 1f, 0.4f);
			break;
		}
	}

	private void OnResearchNodeSelected()
	{
		if (_researchNode == null)
			return;

		_onResearchNodeSelected?.Invoke(_researchNode);
	}
}
