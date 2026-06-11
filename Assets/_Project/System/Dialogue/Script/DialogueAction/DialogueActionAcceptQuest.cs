using UnityEngine;

[System.Serializable]
public class DialogueActionAcceptQuest : DialogueAction
{
    public QuestSo questToAccept;

    public override void Execute()
    {
        QuestManager.Instance.AcceptQuest(questToAccept.QuestID);
    }
}
