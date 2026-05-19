using UnityEngine;
using System.Linq;
[CreateAssetMenu(fileName = "Req_Quest", menuName = "Requirements/Quest Requirement")]
public class QuestRequirement : Requirement
{
    [SerializeField] private string questID;
    [SerializeField] private QuestStateCondition requiredState = QuestStateCondition.ReadyForTurnIn;

    public override bool IsMet()
    {
        // Safety Check
        if (QuestManager.Instance == null) return false;
        
        // 1. Try to find the quest in the active list
        QuestActive quest = QuestManager.Instance.QuestList.Find(q => q.QuestData.QuestID == questID);
        
        // 3. Evaluate based on what state we want
        switch (requiredState)
        {
            case QuestStateCondition.NotStarted:
                return quest == null;
            case QuestStateCondition.Active:
                // Return true if they are still working on it
                return !quest.IsCompleted; 
                
            case QuestStateCondition.ReadyForTurnIn:
                // Return true if they finished the objectives
                return quest.IsCompleted; 
                
            default:
                return false;
        }
    }
}

public enum QuestStateCondition {NotStarted, Active, ReadyForTurnIn}
