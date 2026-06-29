using UnityEngine;
using System.Collections.Generic;

public class EffectPayload
{
    [field: SerializeField] public GameObject User { get; private set; }
    [field: SerializeField] public GameObject Target { get; private set; }
    [field: SerializeField] public Vector3 TargetPosition { get; private set; }

    [field: SerializeField] public Vector2 HitDirection { get; private set; }
    [field: SerializeField] public Vector2 HitImpactPoint { get; private set; }

    public HashSet<IDamagable> HitTargets;
    
    public EffectPayload(GameObject user, GameObject target = null, Vector3 targetPosition = default, Vector2 hitDirection = default, Vector2 hitImpactPoint = default, HashSet<IDamagable> hitTargets = null)
    {
        User = user;
        Target = target;
        TargetPosition = targetPosition;
        HitDirection = hitDirection;
        HitImpactPoint = hitImpactPoint;
        HitTargets = hitTargets;
    }
}
