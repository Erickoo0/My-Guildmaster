using System;
using UnityEngine;

public enum RequirementType { Quest } // You can expand this to: Quest, Item, PlayerLevel, etc.

[System.Serializable]
public class RequirementData
{
    public RequirementType type = RequirementType.Quest;

    // --- QUEST FIELDS ---
    // These will only be used if type == RequirementType.Quest
    public string questID;
    public QuestStateCondition requiredState = QuestStateCondition.ReadyForTurnIn;

    // --- FUTURE ITEM FIELDS (Example) ---
    // public string itemID;
    // public int requiredAmount;

    public bool IsMet()
    {
        switch (type)
        {
            case RequirementType.Quest:
                if (QuestManager.Instance == null) 
                    return requiredState == QuestStateCondition.NotStarted;
                
                QuestActive quest = QuestManager.Instance.QuestList.Find(q => q.QuestData.QuestID == questID);
                bool isFinished = QuestManager.Instance.CompletedQuestList.Contains(questID);
                
                switch (requiredState)
                {
                    case QuestStateCondition.NotStarted: return quest == null && !isFinished;
                    case QuestStateCondition.Active: return quest != null && !quest.IsCompleted; 
                    case QuestStateCondition.ReadyForTurnIn: return quest != null && quest.IsCompleted; 
                    case QuestStateCondition.Finished: return isFinished;
                    default: return false;
                }

            default:
                return false;
        }
    }
}

public enum QuestStateCondition { NotStarted, Active, ReadyForTurnIn, Finished }