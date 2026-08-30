using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;

/// <summary>
/// <summary>
/// A centralized manager that handles drag-and-drop logic and unified tooltips
/// for Inventory Slots and Skill Nodes.
/// </summary>/// </summary>
public class MouseManager : MonoBehaviour
{

	[Header("Drag and Drop References")]
	[SerializeField] private GameObject ghostPanel;
	[SerializeField] private TMP_Text ghostName;
	[SerializeField] private TMP_Text ghostStack;

	[Header("Tooltip References")]
	[SerializeField] private GameObject tooltipPanel;
	[SerializeField] private TMP_Text tooltipName;
	[SerializeField] private TMP_Text tooltipDescription;
	[SerializeField] private Vector2 tooltipOffset = new Vector2(100f, 50f);
	[SerializeField] private Vector2 recipeTooltipOffset = new Vector2(400f, -200f);
	private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
	private Vector2 _currentMousePosition;

	// Raycast Variables
	private PointerEventData _eventData;
	private ItemRecipeUI _hoveredRecipe;

	// Cached References
	private SkillNodeUI _hoveredSkillNode;
	private IItemSlotUI _hoveredSlotUI;
	private IItemSlotUI _sourceSlotUI;

	public static MouseManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Debug.unityLogger.Log("Multiple MouseManagers detected. Disabling script.");
			Destroy(gameObject);
			return;
		}
		Instance = this;

		_eventData = new PointerEventData(EventSystem.current);

		ToggleGhost(false);
		ToggleTooltip(false);
	}

	private void Update()
	{
		if (Pointer.current == null) return;
		_currentMousePosition = Pointer.current.position.ReadValue();

		// 1. Perform a UI raycast to find hovering targetgets
		ScanForMouseTargets();

		// 2. Handle tooltip display based on what we found
		HandleTooltip();

		// 3. Detects Input
		if (Pointer.current.press.wasPressedThisFrame) StartDrag();
		if (Pointer.current.press.isPressed) WhileDragging();
		if (Pointer.current.press.wasReleasedThisFrame) EndDrag();
	}

	/// <summary>
	/// Perform a raycast to populate hovered UI elements
	/// </summary>
	private void ScanForMouseTargets()
	{
		// Reset cached hover states
		_hoveredSlotUI = null;
		_hoveredSkillNode = null;
		_hoveredRecipe = null;

		_eventData.position = _currentMousePosition;
		_raycastResults.Clear();

		// Perform raycast
		EventSystem.current.RaycastAll(_eventData, _raycastResults);

		// Loop through all raycast results
		foreach (var result in _raycastResults)
		{
			if (_hoveredSlotUI == null)
				_hoveredSlotUI = result.gameObject.GetComponentInParent<IItemSlotUI>();

			if (_hoveredSkillNode == null)
				_hoveredSkillNode = result.gameObject.GetComponentInParent<SkillNodeUI>();

			if (_hoveredRecipe == null)
				_hoveredRecipe = result.gameObject.GetComponentInParent<ItemRecipeUI>();

			// If we found a valid target, stop checking
			if (_hoveredSlotUI != null || _hoveredSkillNode != null || _hoveredRecipe != null)
				break;
		}
	}

	private void HandleTooltip()
	{
		// Hide the tooltip if currently dragging an item
		if (_sourceSlotUI != null)
		{
			ToggleTooltip(false);
			return;
		}

		// 1. If hovering an Inventory Slot
		if (_hoveredSlotUI != null && _hoveredSlotUI.itemInstance != null && _hoveredSlotUI.itemInstance.DataSo != null)
		{
			DisplayTooltip(_hoveredSlotUI.itemInstance.DataSo.ItemName, _hoveredSlotUI.itemInstance.DataSo.ItemDescription);
			return;
		}

		// 2. If hovering a Skill Node
		if (_hoveredSkillNode != null)
		{
			DisplayTooltip(_hoveredSkillNode.TooltipTitle, _hoveredSkillNode.TooltipDescription);
			return;
		}

		// 3. If hovering an Item Recipe
		if (_hoveredRecipe != null)
		{
			Vector2 recipePosition = (Vector2)_hoveredRecipe.transform.position + recipeTooltipOffset;
			DisplayTooltip(_hoveredRecipe.TooltipTitle, _hoveredRecipe.TooltipDescription, recipePosition);
			return;
		}

		ToggleTooltip(false);
	}

	/// <summary>
	/// Reusable helper to format and position the tooltip panel
	/// </summary>
	private void DisplayTooltip(string title, string description, Vector2? overridePosition = null)
	{
		tooltipName.text = title;
		tooltipDescription.text = description;

		// Check for position override
		if (overridePosition.HasValue)
		{
			tooltipPanel.transform.position = overridePosition.Value;
		} else
		{
			tooltipPanel.transform.position = _currentMousePosition + tooltipOffset;
		}

		tooltipPanel.transform.SetAsLastSibling();
		ToggleTooltip(true);
	}

	private void StartDrag()
	{
		_sourceSlotUI = _hoveredSlotUI;

		if (_sourceSlotUI != null && _sourceSlotUI.itemInstance != null)
		{
			// Set the text
			ghostName.text = _sourceSlotUI.itemInstance.DataSo.ItemName;
			ghostStack.text = _sourceSlotUI.itemInstance.stackSize.ToString();
			ghostPanel.transform.position = _currentMousePosition;

			// Hide item from Source slot to "pick it up"
			ToggleGhost(true);
		}

		ghostPanel.transform.SetAsLastSibling();
	}

	private void WhileDragging()
	{
		ghostPanel.transform.position = _currentMousePosition;
		if (_sourceSlotUI == null || _sourceSlotUI.itemInstance.DataSo.ItemIcon == null) return;

		ghostPanel.GetComponent<Image>().sprite = GlobalHelper.GetAnimatedSprite(_sourceSlotUI.itemInstance.DataSo);
	}

	private void EndDrag()
	{
		if (_sourceSlotUI == null) return;

		//Find the target slot
		IItemSlotUI targetSlotUI = _hoveredSlotUI;

		// Swap Items Logic
		if (targetSlotUI != null && targetSlotUI != _sourceSlotUI)
		{
			// 1. Handle same-storage swap.
			if (_sourceSlotUI.ItemStorage == targetSlotUI.ItemStorage)
			{
				_sourceSlotUI.ItemStorage.SwapItems(_sourceSlotUI.SlotIndex, targetSlotUI.SlotIndex);
			}
			// 2. Handle cross-storage transfer.
			else
			{
				ItemInstance sourceItem = _sourceSlotUI.ItemStorage.GetItem(_sourceSlotUI.SlotIndex);
				ItemInstance targetItem = targetSlotUI.ItemStorage.GetItem(targetSlotUI.SlotIndex);

				_sourceSlotUI.ItemStorage.SetItem(_sourceSlotUI.SlotIndex, targetItem);
				targetSlotUI.ItemStorage.SetItem(targetSlotUI.SlotIndex, sourceItem);
			}
		}


		// Drop Items Logic
		if (targetSlotUI == null && _sourceSlotUI.ItemStorage.CanDropToWorld)
		{
			// Converts mouse coordinates into 2D world coordinates
			Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(_currentMousePosition);
			mouseWorldPosition.z = 0;
			_sourceSlotUI.ItemStorage.DropItems(_sourceSlotUI.SlotIndex, mouseWorldPosition);
		}

		// Clean up
		ToggleGhost(false);
		_sourceSlotUI = null;

	}

	private void ToggleGhost(bool toggle)
	{
		ghostPanel.SetActive(toggle);
		ghostPanel.GetComponent<Image>().enabled = toggle;
		ghostName.enabled = toggle;
		ghostStack.enabled = toggle;

		if (_sourceSlotUI != null)
			_sourceSlotUI.SetDraggingState(toggle); // Tell the slot to hide its UI

	}

	private void ToggleTooltip(bool toggle)
	{
		tooltipPanel.SetActive(toggle);
		tooltipName.enabled = toggle;
		tooltipDescription.enabled = toggle;
	}
}
