using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MaiTutorialState : BaseNPCOverrideWanderState
{
    public override void Update()
    {
        base.Update();

        // Once the tutorial quest has been accepted, this will return false
        if (!EvaluateRequirements())
        {
            FinishOverride();
        }
    }
}
