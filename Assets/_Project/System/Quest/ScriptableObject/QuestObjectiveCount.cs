using UnityEngine;

[System.Serializable]
public class QuestObjectiveCount: QuestObjectiveBase
{
    [SerializeField] private string targetID;
    [SerializeField] private int requiredAmount;

    public override bool IsCountBased => true;
    public override string TargetID => targetID;
    public override int RequiredAmount => requiredAmount;
}
