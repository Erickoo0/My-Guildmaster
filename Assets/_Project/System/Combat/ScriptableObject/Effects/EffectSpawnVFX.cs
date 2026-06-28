using UnityEngine;

[System.Serializable]
public class EffectSpawnVFX : Effect
{
    [Header("VFX")]
    public GameObject Prefab;
    public bool AlignToHitDirection = true;
    public bool AttachToUser = true;

    public override bool Execute(EffectPayload effectPayload)
    {
        if (Prefab == null) return false;
        
        Vector2 spawnPosition = AttachToUser ? effectPayload.User.transform.position : effectPayload.TargetPosition;
        Transform vfxParent = AttachToUser ? effectPayload.User.transform : null;
        
        // Spawn the VFX
        GameObject vfxInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity, vfxParent);
        Object.Destroy(vfxInstance, 1.0f);
        
        if (AlignToHitDirection && effectPayload.HitDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(effectPayload.HitDirection.y, effectPayload.HitDirection.x) * Mathf.Rad2Deg;
            vfxInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (!AlignToHitDirection && effectPayload.HitDirection != Vector2.zero)
        {
            if (effectPayload.HitDirection.x < 0)
                vfxInstance.transform.rotation = Quaternion.Euler(0, 180f, 0);
            else if (effectPayload.HitDirection.x > 0)
                vfxInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        return true; // Effect successfully fired
    }
    
    public override Effect Clone()
    {
        return new EffectSpawnVFX
        {
            Prefab = Prefab,
            AlignToHitDirection = AlignToHitDirection,
            AttachToUser = AttachToUser
        };
    }
}
