using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager Instance { get; private set; }

    //[SerializeField] private QuestUI questUI;
    [SerializeField] private List<QuestSo> questDatabase = new List<QuestSo>();
    
    private List<QuestActive> _questList = new List<QuestActive>();
    public List<QuestActive> QuestList => _questList;
    
    private List<string> _completedQuestList = new List<string>();
    public List<string> CompletedQuestList => _completedQuestList;
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.unityLogger.Log("Multiple QuestManagers detected. Disabling script.");
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.OnUpdateQuestObjectiveRequested += HandleObjectiveUpdate;
        EventBus.OnEntityDeathRequested += HandleEntityDeath;
        EventBus.OnDialogueEventRequested += HandleDialogueEvent;
    }

    private void OnDisable()
    {
        EventBus.OnUpdateQuestObjectiveRequested -= HandleObjectiveUpdate;
        EventBus.OnEntityDeathRequested -= HandleEntityDeath;
        EventBus.OnDialogueEventRequested -= HandleDialogueEvent;
    }

    private void HandleEntityDeath(GameObject entityRoot)
    {
        if (entityRoot.TryGetComponent(out BaseEntityController entityController))
        {
            string targetID = entityController.GetTargetID();

            if (!string.IsNullOrEmpty(targetID))
                HandleObjectiveUpdate(targetID, 1);
        }
    }

    private void HandleObjectiveUpdate(string targetID, int amount)
    {
        foreach (QuestActive questActive in _questList)
        {
            if (questActive.IsCompleted) continue; // Skip the quest if its already completed
            
            // Look through every objective in the quest, and check if Event target ID matches the objective target ID
            for (int i = 0; i < questActive.QuestData.QuestObjectives.Count; i++)
            {
                
                // If it does, add the amount to the objective progress
                if (questActive.QuestData.QuestObjectives[i].TargetID == targetID)
                {
                    questActive.AddObjectiveProgress(i, amount);
                    //questUI.UpdateQuestUI(questActive);
                    EventBus.RequestUpdateQuest();
                }
            }
        }
    }
    
    private void HandleDialogueEvent(string dialogueEvent, object questData)
    {
        switch (dialogueEvent)
        {
            case "AcceptQuest":
                AcceptQuest(dialogueEvent, questData);
                break;
            case "CompleteQuest":
                CompleteQuest(dialogueEvent, questData);
                break;
        }
    }
    
    private void AcceptQuest(string dialogueEvent, object questData)
    {
        // 1. Filter out non-quest events
        if (dialogueEvent != "AcceptQuest") return;

        // 2. Pattern Match: Try to treat questData as a string. 
        // If it is a string, assign it to the variable 'questID'.
        if (questData is string questID)
        {
            // Check if we already have this quest
            if (_questList.Exists(q => q.QuestData.QuestID == questID)) return;
            
            // Check if the quest has already been completed 
            if (_completedQuestList.Contains(questID)) return;

            // Look it up in the database
            QuestSo questDataSo = GetQuestByID(questID);
        
            if (questDataSo != null)
            {
                _questList.Add(new QuestActive(questDataSo));
                //questUI.AddQuestUI(_questList[^1]); // [^1] is shorthand for 'last index'
                
                // Scan Inventory to immediately update item related quest progress
                foreach (ItemInstance item in InventoryManager.Instance.itemsList)
                {
                    if (item != null && item.DataSo != null)
                    {
                        HandleObjectiveUpdate(item.DataSo.ItemID, item.stackSize);
                    }
                }
                
                EventBus.RequestUpdateQuest();
            }
            else
            {
                Debug.LogError($"Quest ID {questID} not found in database!");
            }
        }
    }
    
    private void CompleteQuest(string dialogueEvent, object questData)
    {
        // Safety check
        if (dialogueEvent != "CompleteQuest") return;
        
        // 1. Pattern Match
        if (questData is string questID)
        {
            // 3. Find the active quest in the current quest list
            QuestActive quest = _questList.Find(q => q.QuestData.QuestID == questID);
            
            // 4. Make sure the quest actually exists and that the player has finished the objective
            if (quest != null && quest.IsCompleted)
            {
                // 5. Remove the quest from the active quest list and add it to the completed quest list
                _questList.Remove(quest);
                _completedQuestList.Add(questID);
                EventBus.RequestUpdateQuest();
            }
            
            else Debug.LogWarning($"Attempted to turn in quest {questID}, but it is either not active or not completed.");
        } 
    }
    
    //----Save Methods----
    public void PopulateSaveData(SaveData saveData)
    {
        // Reset the savedQuests list
        saveData.savedQuests.Clear();

        // Loop through all active quests
        foreach (QuestActive questActive in _questList)
        {
            // Safety Check
            if (questActive == null || questActive.QuestData == null) continue;

            // Create a new SavedQuest object 
            SavedQuest savedQuest = new SavedQuest
            {
                questID = questActive.QuestData.QuestID,
                objectiveProgress = questActive.ObjectiveProgress,
                isCompleted = questActive.IsCompleted
            };
            
            // Add the SavedQuest to the list
            saveData.savedQuests.Add(savedQuest);
        }
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        // Reset the savedQuests list just incase
        _questList.Clear();

        foreach (SavedQuest savedQuest in saveData.savedQuests)
        {
            QuestSo questData = GetQuestByID(savedQuest.questID);

            if (questData == null)
            {
                Debug.LogWarning($"[QuestManager] Could not find quest with ID: {savedQuest.questID} in database.");
                continue;
            }
            
            // Create a new QuestActive object from quest database
            QuestActive questActive = new QuestActive
            (
                questData, 
                savedQuest.objectiveProgress, 
                savedQuest.isCompleted
            );
            
            // Add the QuestActive object to the active quest list
            _questList.Add(questActive);

            EventBus.RequestUpdateQuest();
        }
    }
    
    private QuestSo GetQuestByID(string questID)
    {
        return questDatabase.Find(q => q != null && q.QuestID == questID);
    }
}
