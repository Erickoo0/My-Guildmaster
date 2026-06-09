using UnityEngine;

[System.Serializable]
public class EdwinTutorialState : BaseNPCOverrideWanderState
{
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        
        if (!EvaluateRequirements())
        {
            FinishOverride();
        }
    }
}
