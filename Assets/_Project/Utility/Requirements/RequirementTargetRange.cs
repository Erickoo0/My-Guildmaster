using UnityEngine;

[System.Serializable]
public class RequirementTargetRange : Requirement
{
    public float minimumRange = 0f;
    public float maximumRange = 8f;

    public override bool IsMet(GameObject context = null)
    {
        // Safety Check
        if (context == null) return false;

        // 1. Check if the context is a mob
        if (!context.TryGetComponent(out MobController mob)) return false;
        
        // 2. Check Target Validation
        if (mob.currentTarget == null) return false;
        
        // 3. Perform range check
        float distanceToTarget = Vector2.Distance(mob.transform.position, mob.currentTarget.transform.position);
        return distanceToTarget >= minimumRange && distanceToTarget <= maximumRange;
    }
}
