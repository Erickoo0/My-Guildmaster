using UnityEngine;

[System.Serializable]
public class RequirementGameFlag : Requirement
{
    public FlagKeys.GameFlag requiredGameFlag;

    public bool requiredState = true;

    public override bool IsMet()
    {
        // Get the current state of the required game flag from the manager
        bool currentState = GameFlagManager.Instance.GetGameFlag(requiredGameFlag);
        
        return currentState == requiredState;
    }
}
