using UnityEngine;

[System.Serializable]
public class RequirementGameStat : Requirement
{
    public FlagKeys.GameStat requiredGameStat;

    public int requiredMinimumValue;

    public bool requireEqualValue = false;

    public override bool IsMet()
    {
        int currentValue = GameFlagManager.Instance.GetGameStat(requiredGameStat);
        
        if (requireEqualValue) return currentValue == requiredMinimumValue;
        else return currentValue >= requiredMinimumValue;
    }
}
