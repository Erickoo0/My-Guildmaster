using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Npc : MonoBehaviour, IInteractable
{
    [Header("References")] 
    private NPCController _npcController;
    
    [Header("Dialogue Data")] 
    [SerializeField] private string dialogueName;
    [SerializeField] private Sprite dialoguePortrait;
    [SerializeField] private NPCDialogueData npcDialogueData;
    [SerializeField] private NPCSpeechBubbleData speechBubbleData;
    
    [Header("Cached Daily Dialogue")]
    private DialogueNode _dailyDefaultNode;
    private DialogueNode _dailyHomeNode;
    private DialogueNode _dailySleepNode;
    private DialogueNode _dailyHobbyNode;
    private DialogueNode _dailyWorkNode;
    
    private void OnEnable() => EventBus.OnDayChanged += EvaluateDailyDialogue;
    private void OnDisable() => EventBus.OnDayChanged -= EvaluateDailyDialogue;

    [Header("Shop Data")] 
    [SerializeField] private ItemDataSo[] shopList;
    
    public ItemDataSo[] ShopList => shopList;
    public string  DialogueName => dialogueName;
    public Sprite DialoguePortrait => dialoguePortrait;
    public DialogueNode CurrentDialogueNode => GetDialogueForCurrentState();
    public string[] CurrentSpeechBubble => GetSpeechBubbleForCurrentState();
    
    private void Start()
    {
        _npcController = GetComponent<NPCController>();
        // Evaluate the daily dialogue immediately for the first time
        EvaluateDailyDialogue(this, TimeSpan.Zero);
    }

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
    
    private void EvaluateDailyDialogue(object sender, TimeSpan time)
    {
        // Pass the DialogueNode Array to the SelectNode method for each state
        _dailyDefaultNode = SelectNode(npcDialogueData.DefaultDialogueNode);
        _dailyHomeNode = SelectNode(npcDialogueData.HomeDialogueNode);
        _dailySleepNode = SelectNode(npcDialogueData.SleepDialogueNode);
        _dailyHobbyNode = SelectNode(npcDialogueData.HobbyDialogueNode);
        _dailyWorkNode = SelectNode(npcDialogueData.WorkDialogueNode);
    }

    // Returns a DialogueNode from the given DialogueNode array
    private DialogueNode SelectNode(DialogueNode[] nodes)
    {
        // Safety Check
        if (nodes == null || nodes.Length == 0) return null;
        
        // 1. Only return nodes that have all their requirements met
        List<DialogueNode> validNodes = nodes.Where(n => n.requirements.All(r => r.IsMet())).ToList();
        // If no nodes were found, return null
        if (validNodes.Count == 0) return null;
        
        // 2. Check for any important nodes
        DialogueNode importantNode = validNodes.FirstOrDefault(n => n.isImportant);
        // If it exists, return it
        if (importantNode != null) return importantNode;
        
        // 3. Otherwise, return a random node through weighted probabilities
        float totalWeight = validNodes.Sum(n => n.selectionWeight);
        float randomValue = Random.Range(0, totalWeight);
        float currentWeight = 0f;
        foreach (DialogueNode node in validNodes)
        {
            currentWeight += node.selectionWeight;
            if (randomValue <= currentWeight) return node;
        }
        
        // Fallback if random node was not selected for some reason
        return validNodes[0];
    }
    
    private DialogueNode GetDialogueForCurrentState()
    {
        // If we have an Override State active, prioritize its dialogue
        if (_npcController.IsOverrideState && _npcController.OverrideState is IStateOverrider stateOverrider)
        {
            return stateOverrider.GetDialogue();
        }
        
        // 1. Get the Schedule Controller
        var scheduleController = GetComponent<NPCScheduleController>();
        if (scheduleController == null) return _dailyDefaultNode;
        
        var scheduledState = scheduleController.CurrentScheduledState;

        // 2. Switch based on the scheduled state reference
        if (scheduledState == _npcController.WorkState) return _dailyWorkNode ?? _dailyDefaultNode;
        if (scheduledState == _npcController.HomeState) return _dailyHomeNode ?? _dailyDefaultNode;
        if (scheduledState == _npcController.HobbyState) return _dailyHobbyNode ?? _dailyDefaultNode;
        if (scheduledState == _npcController.SleepState) return _dailySleepNode ?? _dailyDefaultNode;

        return _dailyDefaultNode;
        
    }

    private string[] GetSpeechBubbleForCurrentState()
    {
        // 1. Get the Schedule Controller
        var scheduleController = GetComponent<NPCScheduleController>();
        if (scheduleController == null) return speechBubbleData.DefaultSpeechBubbles;
        
        var scheduledState = scheduleController.CurrentScheduledState;
        
        if (scheduledState == _npcController.WorkState) return speechBubbleData.WorkSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.HomeState) return speechBubbleData.HomeSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.HobbyState) return speechBubbleData.HobbySpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        if (scheduledState == _npcController.SleepState) return speechBubbleData.SleepSpeechBubbles ?? speechBubbleData.DefaultSpeechBubbles;
        
        return speechBubbleData.DefaultSpeechBubbles;
    }
}

[System.Serializable]
public class NPCDialogueData
{
    // Currently expanding from single DialogueNode to array
    public DialogueNode[] DefaultDialogueNode;
    public DialogueNode[] HomeDialogueNode;
    public DialogueNode[] SleepDialogueNode;
    public DialogueNode[] HobbyDialogueNode;
    public DialogueNode[] WorkDialogueNode;
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