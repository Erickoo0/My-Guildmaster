using UnityEngine;

[System.Serializable]
public class DialogueActionSetGameFlag : DialogueAction
{
    public FlagKeys.GameFlag flagToSet;
    public bool stateToSet;
    
    public override void Execute()
    {
        GameFlagManager.Instance.SetGameFlag(flagToSet, stateToSet);
    }
}
