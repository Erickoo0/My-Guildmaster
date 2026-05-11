using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Npc : MonoBehaviour, IInteractable
{
    [Header("References")] 
    private NPCController _npcController;
    
    [Header("Dialogue Data")] 
    [SerializeField] private string dialogueName;
    [SerializeField] private Sprite dialoguePortrait;
    [SerializeField] private NPCDialogueData npcDialogueData;

    [Header("Shop Data")] 
    [SerializeField] private ItemDataSo[] shopList;
    
    public ItemDataSo[] ShopList => shopList;
    
    // Properties so DialogueManager can read the private variables
    public string  DialogueName => dialogueName;
    public Sprite DialoguePortrait => dialoguePortrait;
    public DialogueNode CurrentDialogueNode => GetDialogueForCurrentState();
    
    private void Start() => _npcController = GetComponent<NPCController>();
    
    public bool CanInteract()
    {
        return true;
    }
    //
    public void Interact(PlayerController playerController)
    {
        if (!CanInteract()) return;
        DialogueManager.Instance.StartDialogue(this, playerController);
    }
    
    private DialogueNode GetDialogueForCurrentState()
    {
        // 1. Get the Schedule Controller
        var scheduleController = GetComponent<NPCScheduleController>();
        if (scheduleController == null) return npcDialogueData.DefaultDialogueNode;
        
        var scheduledState = scheduleController.CurrentScheduledState;

        // 2. Switch based on the scheduled state reference
        if (scheduledState == _npcController.WorkState)
            return npcDialogueData.WorkDialogueNode ?? npcDialogueData.DefaultDialogueNode;
    
        if (scheduledState == _npcController.HomeState)
            return npcDialogueData.HomeDialogueNode ?? npcDialogueData.DefaultDialogueNode;
    
        if (scheduledState == _npcController.HobbyState)
            return npcDialogueData.HobbyDialogueNode ?? npcDialogueData.DefaultDialogueNode;
    
        if (scheduledState == _npcController.SleepState)
            return npcDialogueData.SleepDialogueNode ?? npcDialogueData.DefaultDialogueNode;

        return npcDialogueData.DefaultDialogueNode;
    }
}

[System.Serializable]
public class NPCDialogueData
{
    public DialogueNode DefaultDialogueNode;
    public DialogueNode HomeDialogueNode;
    public DialogueNode SleepDialogueNode;
    public DialogueNode HobbyDialogueNode;
    public DialogueNode WorkDialogueNode;
}