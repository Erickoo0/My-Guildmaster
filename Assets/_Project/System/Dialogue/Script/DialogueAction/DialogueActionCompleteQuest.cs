using UnityEngine;

[System.Serializable]
public class DialogueActionCompleteQuest : DialogueAction
{
    public QuestSo questToComplete;
    
    public override void Execute()
    {
        QuestManager.Instance.CompleteQuest(questToComplete.QuestID);
    }
}
