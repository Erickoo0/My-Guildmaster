using UnityEngine;

public class EffectPayload
{
    public GameObject User;
    public GameObject Target;
    public Vector3 TargetPosition;
    public float PotencyMultiplier = 1f;

    public EffectPayload(GameObject user, GameObject target = null, Vector3 targetPosition = default)
    {
        User = user;
        Target = target;
        TargetPosition = targetPosition;
    }
}
