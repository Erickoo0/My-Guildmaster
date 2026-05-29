using UnityEngine;

[System.Serializable]
public class RequirementGameStat : Requirement
{
    public FlagKeys.GameStat requiredGameStat;

    public int requiredMinimumValue;

    public override bool IsMet()
    {
        int currentValue = GameFlagManager.Instance.GetGameStat(requiredGameStat);
        return currentValue >= requiredMinimumValue;
    }
}
