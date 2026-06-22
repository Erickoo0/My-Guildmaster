using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectSpawnExplosion : Effect
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionLifetime = 0.5f;
    [SerializeField] private float explosionScale = 1f;
    
    [Header("Explosion Impact Settings")]
    [SerializeReference, SubclassSelector] public List<Effect> explosionEffects = new List<Effect>();

    public override bool Execute(EffectPayload effectPayload)
    {
        if (explosionPrefab == null) return false;
        
        // 1. Determine spawn location
        Vector3 spawnPosition = effectPayload.HitImpactPoint != Vector2.zero ? (Vector3)effectPayload.HitImpactPoint : effectPayload.TargetPosition;
        
        // 2. Spawn the explosion
        GameObject explosionInstance = Object.Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        Object.Destroy(explosionInstance, explosionLifetime);
        
        // 3. Pass the data
        if (explosionInstance.TryGetComponent(out HitBoxAOE hitBox))
        {
            hitBox.Setup(
                user: effectPayload.User,
                effects: explosionEffects,
                maxHits: 999,                              
                hitOnce: true,                             
                destroyOnMax: false,                       
                inheritedTargets: effectPayload.HitTargets // Pass the memory chain!
                );
            
            hitBox.enableHitbox = true;
            return true;
        }
        
        return false;
    }
}
