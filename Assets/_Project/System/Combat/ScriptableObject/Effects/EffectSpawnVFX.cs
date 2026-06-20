using UnityEngine;

[System.Serializable]
public class EffectSpawnVFX : Effect
{
    [Header("VFX")]
    public GameObject vfxPrefab;
    public bool alignToHitDirection = true;

    public override bool Execute(EffectPayload effectPayload)
    {
        if (vfxPrefab == null) return false;

        Vector2 spawnPosition = effectPayload.TargetPosition;
        
        // Spawn the VFX
        GameObject vfxInstance = Object.Instantiate(vfxPrefab, spawnPosition, Quaternion.identity);
        Object.Destroy(vfxInstance, 1.0f);
        
        if (alignToHitDirection && effectPayload.HitDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(effectPayload.HitDirection.y, effectPayload.HitDirection.x) * Mathf.Rad2Deg;
            vfxInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (!alignToHitDirection && effectPayload.HitDirection != Vector2.zero)
        {
            if (effectPayload.HitDirection.x < 0)
                vfxInstance.transform.rotation = Quaternion.Euler(0, 180f, 0);
            else if (effectPayload.HitDirection.x > 0)
                vfxInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        return true; // Effect successfully fired
    }
}
