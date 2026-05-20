using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Req_Quest", menuName = "Requirements/Quest Requirement")]
public class QuestRequirement : Requirement
{
    [SerializeField] private string questID;
    [SerializeField] private QuestStateCondition requiredState = QuestStateCondition.ReadyForTurnIn;

    public override bool IsMet()
    {
        // Race Condition Safety Check
        if (QuestManager.Instance == null) 
        {
            // If the manager hasn't loaded yet, assume the quest hasn't started.
            return requiredState == QuestStateCondition.NotStarted;
        }
        
        // 1. Try to find the quest in the active list
        QuestActive quest = QuestManager.Instance.QuestList.Find(q => q.QuestData.QuestID == questID);
        
        // 2. Check if the quest has already been completed and turned in
        bool isFinished = QuestManager.Instance.CompletedQuestList.Contains(questID);
        
        // 3. Evaluate based on what state we want
        switch (requiredState)
        {
            case QuestStateCondition.NotStarted:
                // Returns true if not in active or completed quest list
                return quest == null && !isFinished;
            
            case QuestStateCondition.Active:
                // Return true if they are still working on it
                return quest != null && !quest.IsCompleted; 
                
            case QuestStateCondition.ReadyForTurnIn:
                // Return true if they finished the objectives
                return quest != null && quest.IsCompleted; 
            
            case QuestStateCondition.Finished:
                return isFinished;
            
            default:
                return false;
        }
    }
}

public enum QuestStateCondition {NotStarted, Active, ReadyForTurnIn, Finished}
