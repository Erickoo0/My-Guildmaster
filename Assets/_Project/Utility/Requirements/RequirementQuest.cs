using System;
using UnityEngine;

[System.Serializable]
public class RequirementQuest : Requirement
{
    [Header("Quest Fields")]
    public string questID;
    public QuestStateCondition requiredState = QuestStateCondition.ReadyForTurnIn;

    public override bool IsMet()
    {
        // Safety Check
        if (QuestManager.Instance == null) 
            return requiredState == QuestStateCondition.NotStarted;
        
        // 1. Check if the quest is active or completed
        bool isActive = QuestManager.Instance.TryGetActiveQuest(questID, out QuestActive quest);
        bool isCompleted = QuestManager.Instance.CheckQuestCompletion(questID);
        
        switch (requiredState)
        {
            case QuestStateCondition.NotStarted: return quest == null && !isCompleted;
            case QuestStateCondition.Active: return quest != null && !quest.IsCompleted; 
            case QuestStateCondition.ReadyForTurnIn: return quest != null && quest.IsCompleted; 
            case QuestStateCondition.Finished: return isCompleted;
            default: return false;
        }
    }
}

public enum QuestStateCondition { NotStarted, Active, ReadyForTurnIn, Finished }