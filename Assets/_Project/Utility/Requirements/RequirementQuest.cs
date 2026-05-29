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
        
        // 1. Check if the quest is in the active quest list
        QuestActive quest = QuestManager.Instance.QuestList.Find(q => q.QuestData.QuestID == questID);
        
        // 2. Check if the quest is in the completed quest list
        bool isFinished = QuestManager.Instance.CompletedQuestList.Contains(questID);
        
        switch (requiredState)
        {
            case QuestStateCondition.NotStarted: return quest == null && !isFinished;
            case QuestStateCondition.Active: return quest != null && !quest.IsCompleted; 
            case QuestStateCondition.ReadyForTurnIn: return quest != null && quest.IsCompleted; 
            case QuestStateCondition.Finished: return isFinished;
            default: return false;
        }
    }
}

public enum QuestStateCondition { NotStarted, Active, ReadyForTurnIn, Finished }