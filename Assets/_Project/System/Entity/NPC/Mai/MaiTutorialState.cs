using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MaiTutorialState : BaseNPCOverrideWanderState
{
    [SerializeField] private Requirement[] requirements;

    protected override List<string> GetPOITargetIDs()
    {
        return new List<string> { "Grand_Tree_Front" };
    }

    public override bool EvaluateRequirements()
    {
        if (requirements == null || requirements.Length == 0) return false;
        
        // "All" only returns true if every requirement in the List returns true
        return requirements.All(r => r.IsMet());
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
