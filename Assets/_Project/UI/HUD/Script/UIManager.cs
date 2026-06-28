using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;


public class UIManager : MonoBehaviour
{
    private Stack<GameObject> _panelStack = new Stack<GameObject>();
    
    private void OnEnable()
    {
        EventBus.OnMenuOpenRequested += HandleMenuOpenRequest;
        EventBus.OnMenuCloseRequested += HandleMenuCloseRequest;
    }
    
    private void OnDisable()
    {
        EventBus.OnMenuOpenRequested -= HandleMenuOpenRequest;
        EventBus.OnMenuCloseRequested -= HandleMenuCloseRequest;
    }

    private void HandleMenuOpenRequest(GameObject menu)
    {
        // Safety Check
        if (_panelStack.Contains(menu)) return;
        
        // 1. If first menu is opening, lock player and switch to UI Input Map
        if (_panelStack.Count == 0)
        {
            PauseManager.SetPause(true);
            EventBus.RequestPlayerMovementToggle(false);
            
            PlayerInputManager.Instance.PlayerInput.SwitchCurrentActionMap("UI");

        }
        
        // 2. Set Menu and add to stack (if pause menu is not active)
        if (PauseManager.Instance.PauseMenuPanel.activeSelf && menu != PauseManager.Instance.PauseMenuPanel) return;
        
        menu.SetActive(true);
        menu.transform.SetAsLastSibling(); // Set the menu to be the last sibling for draw order
        _panelStack.Push(menu);
    }

    // When the ESC key gets pressed, send a close request
    public void OnCloseMenuInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Guard Clause to stop the input from firing 3 times
        
        if (_panelStack.Count > 0) HandleMenuCloseRequest();
    }

    private void HandleMenuCloseRequest(GameObject menu = null)
    {
        if (_panelStack.Count <= 0) return;

        GameObject closedMenu = null;
    
        // 1. Remove top menu if no specific menu is requested
        if (menu == null)
        {
            closedMenu = _panelStack.Pop();
            closedMenu.SetActive(false);
        }
        else // 2. Remove specific menu
        {
            closedMenu = menu;
            if (_panelStack.Peek() == menu) 
            {
                _panelStack.Pop();
            }
            else 
            {
                // CRITICAL FIX: Stacks iterate backwards. You must reverse the list 
                // before rebuilding the stack, or your UI draw order will flip forever!
                List<GameObject> tempStack = new List<GameObject>(_panelStack);
                tempStack.Remove(menu); 
                tempStack.Reverse(); 
                _panelStack = new Stack<GameObject>(tempStack); 
            }
            menu.SetActive(false);
        }
    
        // 3. Broadcast that a specific menu was closed
        if (closedMenu != null)
            EventBus.NotifyMenuClosed(closedMenu);
    
        // 4. If no menus left, return to gameplay
        if (_panelStack.Count <= 0)
        {
            // CRITICAL FIX: Unlock the player and unpause BEFORE switching maps
            EventBus.RequestPlayerMovementToggle(true);
            PauseManager.SetPause(false);
        
            // THIS MUST BE THE ABSOLUTE LAST LINE TO PREVENT UNITY THREAD ABORTS
            PlayerInputManager.Instance.PlayerInput.SwitchCurrentActionMap("Player");
        }
    }
}