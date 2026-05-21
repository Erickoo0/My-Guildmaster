using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MaiTutorialState : BaseNPCOverrideWanderState
{
    protected override List<string> GetPOITargetIDs()
    {
        return new List<string> { "Grand_Tree_Front" };
    }

    public override bool EvaluateRequirements()
    {
        if (requirements == null || requirements.Count == 0) return false;
        
        // Check if any requirement has not been met
        for (int i = 0; i < requirements.Count; i++)
        {
            if (!requirements[i].IsMet()) return false;
        }
        
        // If all requirements are met, then return true
        return true;
    }

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
