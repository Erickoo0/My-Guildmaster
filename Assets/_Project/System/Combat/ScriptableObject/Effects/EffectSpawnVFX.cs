using UnityEngine;

public class EffectSpawnVFX : Effect
{
    [Header("VFX")]
    public GameObject vfxPrefab;
    public bool alignToHitDirection = true;

    public override bool Execute(EffectPayload effectPayload)
    {
        if (vfxPrefab == null) return false;
        
        Vector2 spawnPosition = effectPayload.HitImpactPoint != Vector2.zero 
            ? effectPayload.HitImpactPoint 
            : (Vector2)effectPayload.TargetPosition;
        
        // Spawn the VFX
        GameObject vfxInstance = Object.Instantiate(vfxPrefab, spawnPosition, Quaternion.identity);
        Object.Destroy(vfxInstance, 1.0f);
        // Align the rotation if requested and we have a valid direction
        if (alignToHitDirection && effectPayload.HitDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(effectPayload.HitDirection.y, effectPayload.HitDirection.x) * Mathf.Rad2Deg;
            vfxInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        return true; // Effect successfully fired
    }
}
