using UnityEngine;

[System.Serializable]
public abstract class QuestObjectiveBase
{
    [SerializeField] private string objectiveTitle = "Objective Text";
    public string ObjectiveTitle => objectiveTitle;
    
    // Is this objective tracked via event counters (Items/Kills) or static world conditions?
    public abstract bool IsCountBased { get; }
    public abstract int RequiredAmount { get; }
    
    // Used by Count-Based Objectives
    public virtual string TargetID => string.Empty;

    // Used by State-Based Objectives (Requirements)
    public virtual bool IsConditionMet() => false;
}
