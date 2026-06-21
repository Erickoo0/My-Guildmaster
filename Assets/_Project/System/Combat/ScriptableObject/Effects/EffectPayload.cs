using UnityEngine;
using System.Collections.Generic;

public class EffectPayload
{
    public GameObject User;
    public GameObject Target;
    public Vector3 TargetPosition;

    public Vector2 HitDirection;
    public Vector2 HitImpactPoint;

    public HashSet<IDamagable> HitTargets;
    
    public EffectPayload(GameObject user, GameObject target = null, Vector3 targetPosition = default, Vector2 hitDirection = default, Vector2 hitImpactPoint = default, HashSet<IDamagable> hitTargets = null)
    {
        User = user;
        Target = target;
        TargetPosition = targetPosition;
        HitDirection = hitDirection;
        HitImpactPoint = hitImpactPoint;
        HitTargets = HitTargets;
    }
}
