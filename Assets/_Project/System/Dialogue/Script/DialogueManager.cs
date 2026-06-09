using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    // References
    private DialogueUI _dialogueUI;
    private DialogueOptionController _dialogueOptionController;
    
    private Npc _currentSpeaker;
    private DialogueGroup _currentDialogueGroup;
    private DialogueNode _currentDialogueNode;
    private int _currentLineIndex;
    private bool _isWaitingChoice = false;

    private void OnEnable() => EventBus.OnMenuClosed += HandleForcedClose;
    private void OnDisable() => EventBus.OnMenuClosed -= HandleForcedClose;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.unityLogger.Log("Multiple DialogueManagers detected. Disabling script.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Set the references
        _dialogueUI = GetComponent<DialogueUI>();
        _dialogueOptionController = GetComponent<DialogueOptionController>();
   }
    
    private void Update()
    {
        // FOCUS GUARD: If options are shown, ensure the keyboard never "loses" the selection
        if (_isWaitingChoice && EventSystem.current.currentSelectedGameObject == null)
        {
            GameObject first = _dialogueOptionController.GetFirstButton();
            if (first != null) EventSystem.current.SetSelectedGameObject(first);
        }
    }

    public void StartDialogue(Npc speaker, PlayerController playerController)
    {
        // Guard Clause
        if (_currentDialogueNode != null) return;
        
        // Set the starting data
        _currentSpeaker = speaker;
        
        _currentDialogueGroup = _currentSpeaker.CurrentDialogueGroup;
        if (_currentDialogueGroup == null) return;

        _currentDialogueNode = _currentDialogueGroup.GetStartingNode();
        if (_currentDialogueNode == null) return;
        
        _currentLineIndex = 0;
        _isWaitingChoice = false;
        
        _dialogueOptionController.ClearOptions();
        
        EventBus.RequestOpenMenu(_dialogueUI.DialoguePanel);
        UpdateDisplay();
    }

    public void OnAdvanceDialogueInput(InputAction.CallbackContext context)
    {
        // Guard Clause
        if (_currentDialogueNode == null || !context.performed || !_dialogueUI.IsVisible) return;
        
        // 1. If still typing, finish instantly
        if (_dialogueUI.IsTyping)
        {
            _dialogueUI.FinishLineEarly();
        }
        // 2. If waiting for option selection, do nothing
        else if (_isWaitingChoice)
        {
            return;
        }
        // 3. Continue
        else
        {
            ContinueDialogue();
        }
    }

    private void ContinueDialogue()
    {
        // Advance the line index
        _currentLineIndex++;

        // Update the display to show new line index
        if (_currentLineIndex < _currentDialogueNode.dialogueLines.Length)
        {
            UpdateDisplay();
        }
        else
        {
            // Close the dialogue system if we have reached the end
            CloseDialogue();
        }
    }

    private void UpdateDisplay()
    {
        // Tell the Dialogue UI class to update the UI with the new line
        string currentLine = _currentDialogueNode.dialogueLines[_currentLineIndex];
        _dialogueUI.UpdateUI(_currentSpeaker.DialogueName, currentLine, _currentSpeaker.DialoguePortrait);
        
        // Check if we are on the last line
        bool isLastLine = _currentLineIndex == _currentDialogueNode.dialogueLines.Length - 1;
        
        // Loop through and execute all node events 
        if (isLastLine && _currentDialogueNode.nodeEvents != null)
        {
            foreach (NodeEventData nodeEvent in _currentDialogueNode.nodeEvents)
                if (!string.IsNullOrEmpty(nodeEvent.eventName))
                    HandleDialogueEvents(nodeEvent.eventName, nodeEvent.eventParameter);
        }
        
        CheckForOptions();
    }

    private void CheckForOptions()
    {
        // Check if dialogue is on the last line
        bool isLastLine = _currentLineIndex == _currentDialogueNode.dialogueLines.Length - 1;
        bool hasOptions = _currentDialogueNode.dialogueOptions != null && _currentDialogueNode.dialogueOptions.Length > 0;

        // Tell the Option Controller to create buttons if on the last line
        if (isLastLine && hasOptions)
        {
            _isWaitingChoice = true;
            _dialogueOptionController.CreateButtons(_currentDialogueNode.dialogueOptions, OnOptionSelected); 
            
            // Find the first button
            GameObject firstButton = _dialogueOptionController.GetFirstButton();
            if (firstButton != null)
            {
                // Set it as selected / focus using Unity EventSystem
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
        }
    }

    // Callback Function passed to the Buttons (activates on button click)
    private void OnOptionSelected(DialogueOption selectedOption)
    {
        // Tell the Option Controller to delete the options
        _isWaitingChoice = false;
        _dialogueOptionController.ClearOptions();

        // 1. Execute the options event if it has one
        if (!string.IsNullOrEmpty(selectedOption.dialogueEvent))
        {
            HandleDialogueEvents(selectedOption.dialogueEvent, selectedOption.eventParameter);
        }
        // 2. Advance to the next node through its nodeID
        if (!string.IsNullOrEmpty(selectedOption.targetNodeID)) 
        {
            DialogueNode nextNode = _currentDialogueGroup.GetNodeByID(selectedOption.targetNodeID);
            if (nextNode != null)
            {
                _currentDialogueNode = nextNode;
                _currentLineIndex = 0;
                UpdateDisplay();
            }
            else
            {
                Debug.LogError($"DialogueManager: Could not find node with ID {selectedOption.targetNodeID}");
                CloseDialogue();
            }
        }
        else // 3. Close the dialogue if there is no next node
        {
            CloseDialogue();
        }
    }

    private void HandleDialogueEvents(string eventName, string eventParameter = null)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        switch (eventName)
        {
        case "OpenShop":
            EventBus.RequestDialogueEvent(eventName, _currentSpeaker.ShopList);
            break;

        case "SetGameFlag":
            if (string.IsNullOrEmpty(eventParameter))
            {
                Debug.LogError("'SetGameFlag' event fired but is missing a parameter");
                break;
            }
            
            if (System.Enum.TryParse(eventParameter, out FlagKeys.GameFlag flagKey))
                GameFlagManager.Instance.SetGameFlag(flagKey, true);
            else 
                Debug.LogError($"DialogueManager: Failed to parse '{eventParameter}' into a valid FlagKeys.GameFlag enum!");
            break;

        case "IncrementGameStat":
            if (string.IsNullOrEmpty(eventParameter))
            {
                Debug.LogError("'IncrementGameStat' event fired but is missing a parameter");
                break; // FIX: Prevents the code from trying to parse a null string below
            }
            
            if (System.Enum.TryParse(eventParameter, out FlagKeys.GameStat statKey))
                GameFlagManager.Instance.IncrementGameStat(statKey, 1);
            else 
                Debug.LogError($"DialogueManager: Failed to parse '{eventParameter}' into a valid FlagKeys.GameStat enum!");
            break;

        default:
            // For everything else (AcceptQuest, CompleteQuest, GiveItem, etc.)
            // Just broadcast it! Let the receiving managers figure out if they care about it.
            EventBus.RequestDialogueEvent(eventName, eventParameter);
            break;
        }
    }
    
    private void CloseDialogue()
    {
        EventBus.RequestCloseMenu(_dialogueUI.DialoguePanel);      
        ResetDialogueState(); // Clean up logic instantly!
    }

    private void HandleForcedClose(GameObject closedMenu)
    {
        if (closedMenu == _dialogueUI.DialoguePanel)
        {
            ResetDialogueState();
        }
    }

    private void ResetDialogueState()
    {
        _currentLineIndex = 0;
        _currentDialogueGroup = null;
        _currentDialogueNode = null;
        _isWaitingChoice = false;
        _dialogueOptionController.ClearOptions();
        _currentSpeaker = null;
    }
}
