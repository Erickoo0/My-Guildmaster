using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Image = UnityEngine.UI.Image;
using TMPro;

/// <summary>
/// A centralized manager that handles drag-and-drop logic for any <see cref="IStorageSlot"/>.
/// </summary>
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [Header("Drag and Drop References")] 
    [SerializeField] private GameObject ghostPanel; 
    [SerializeField] private TMP_Text ghostName;
    [SerializeField] private TMP_Text ghostStack;
    
    [Header("Tooltip References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipName;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(100f, 50f);
        
    private IStorageSlot _sourceSlot;
    private Vector2 _currentMousePosition;
    
    // Raycast Variables
    private PointerEventData _eventData;
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    
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

        // Updates mouse position
        _currentMousePosition = Pointer.current.position.ReadValue();
        
        // Find what the mouse is hovering over
        IStorageSlot hoveredSlot = GetSlotUnderMouse();
        
        // Handle tooltip logic
        HandleTooltip(hoveredSlot);
        
        // Detects Input
        if (Pointer.current.press.wasPressedThisFrame) StartDrag();
        if (Pointer.current.press.isPressed) WhileDragging();
        if (Pointer.current.press.wasReleasedThisFrame) EndDrag();
    }

    private void HandleTooltip(IStorageSlot hoveredSlot)
    {
        // Hide the tooltip if currently dragging an item
        if (_sourceSlot != null)
        {
            ToggleTooltip(false);
            return;
        }

        // If hovering over a valid slot with an item, show tooltip
        if (hoveredSlot != null && hoveredSlot.itemInstance != null && hoveredSlot.itemInstance.DataSo != null)
        {
            tooltipName.text = hoveredSlot.itemInstance.DataSo.ItemName;
            tooltipDescription.text = hoveredSlot.itemInstance.DataSo.ItemDescription;
            tooltipPanel.transform.position = _currentMousePosition + tooltipOffset;
            tooltipPanel.transform.SetAsLastSibling();
            ToggleTooltip(true);
        }
        else
        {
            ToggleTooltip(false);
        }
    }

    private void StartDrag()
    {
        _sourceSlot = GetSlotUnderMouse();

        if (_sourceSlot != null && _sourceSlot.itemInstance != null)
        {
            // Set the text
            ghostName.text = _sourceSlot.itemInstance.DataSo.ItemName;
            ghostStack.text = _sourceSlot.itemInstance.stackSize.ToString();
            ghostPanel.transform.position = _currentMousePosition;
            
            // Hide item from source slot to "pick it up"
            ToggleGhost(true);
        }
        
        ghostPanel.transform.SetAsLastSibling();
    }

    private void WhileDragging()
    {
        ghostPanel.transform.position = _currentMousePosition;
        if (_sourceSlot == null || _sourceSlot.itemInstance.DataSo.ItemIcon == null) return; 
        
        ghostPanel.GetComponent<Image>().sprite = GlobalHelper.GetAnimatedSprite(_sourceSlot.itemInstance.DataSo);
    }

    private void EndDrag()
    {
        if (_sourceSlot == null) return;

        //Find the target slot
        IStorageSlot targetSlot = GetSlotUnderMouse();

        // Swap Items Logic
        if (targetSlot != null && targetSlot != _sourceSlot)
        {
            InventoryManager.Instance.SwapItems(_sourceSlot.Index, targetSlot.Index);
        }
        
        // Drop Items Logic
        if (targetSlot == null)
        {
            // Converts mouse coordinates into 2D world coordinates
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(_currentMousePosition);
            mouseWorldPosition.z = 0;
            InventoryManager.Instance.DropItems(_sourceSlot.Index, mouseWorldPosition);
        }
        
        // Clean up
        ToggleGhost(false);
        _sourceSlot = null;
 
    }

    private void ToggleGhost(bool toggle)
    {
        ghostPanel.SetActive(toggle);
        ghostPanel.GetComponent<Image>().enabled = toggle;
        ghostName.enabled = toggle;
        ghostStack.enabled = toggle;

        if (_sourceSlot != null)
        { 
            _sourceSlot.SetDraggingState(toggle); // Tell the slot to hide its UI
        }
    }
    
    private void ToggleTooltip(bool toggle)
    {
        tooltipPanel.SetActive(toggle);
        tooltipName.enabled = toggle;
        tooltipDescription.enabled = toggle;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private IStorageSlot GetSlotUnderMouse()
    {
        _eventData.position = _currentMousePosition;
        _raycastResults.Clear(); // Clear the old results instead of making a new list
    
        EventSystem.current.RaycastAll(_eventData, _raycastResults);

        foreach (var result in _raycastResults)
        {
            IStorageSlot slot = result.gameObject.GetComponentInParent<IStorageSlot>();
            if (slot != null) return slot;
        }
        return null;
    }
}
