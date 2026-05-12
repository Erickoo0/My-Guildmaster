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
    [SerializeField] private NPCSpeechBubbleData speechBubbleData;

    [Header("Shop Data")] 
    [SerializeField] private ItemDataSo[] shopList;
    
    public ItemDataSo[] ShopList => shopList;
    
    // Properties so DialogueManager can read the private variables
    public string  DialogueName => dialogueName;
    public Sprite DialoguePortrait => dialoguePortrait;
    public DialogueNode CurrentDialogueNode => GetDialogueForCurrentState();
    public string[] CurrentSpeechBubble => GetSpeechBubbleForCurrentState();
    
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

    private string[] GetSpeechBubbleForCurrentState()
    {
        // 1. Get the Schedule Controller
        var scheduleController = GetComponent<NPCScheduleController>();
        if (scheduleController == null) return speechBubbleData.DefaultSpeechBubbles;
        
        var scheduledState = scheduleController.CurrentScheduledState;
        
        if (scheduledState == _npcController.WorkState) 
            return speechBubbleData.WorkSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.HomeState) 
            return speechBubbleData.HomeSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.HobbyState) 
            return speechBubbleData.HobbySpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.SleepState)
            return speechBubbleData.SleepSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        
        return speechBubbleData.DefaultSpeechBubbles;
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

[System.Serializable]
public class NPCSpeechBubbleData
{
    public string[] DefaultSpeechBubbles;
    public string[] HomeSpeechBubbles;
    public string[] SleepSpeechBubbles;
    public string[] HobbySpeechBubbles;
    public string[] WorkSpeechBubbles;
}