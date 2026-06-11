using UnityEngine;

[System.Serializable]
public class DialogueActionIncrementStat : DialogueAction
{
    public FlagKeys.GameStat statToIncrement;
    public int amount = 1;

    public override void Execute()
    {
        GameFlagManager.Instance.IncrementGameStat(statToIncrement, amount);
    }
}
