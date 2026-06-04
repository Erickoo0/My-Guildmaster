using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectSpawnProjectile : Effect
{
    [Header("Projectile Prefab")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileScale = 1f;
    
    [Header("Flight Physics")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileHeight = 0f;
    [SerializeField] private AnimationCurve projectileCurve;

    [Header("Hitbox Settings")]
    [SerializeField] private int maxEnemiesHit = 1;
    [SerializeField] private bool hitOncePerTarget = true;
    [SerializeField] private bool destroyOnMaxHits = true;
    
    [Header("Impact Effects")]
    [SerializeReference, SubclassSelector] public List<Effect> onHitEffects = new List<Effect>();

    public override bool Execute(EffectPayload effectPayload)
    {
        if (projectilePrefab == null) return false;
        
        // 1. Find the user's firepoint component if they have one
        FirePoint firepoint = effectPayload.User.GetComponentInChildren<FirePoint>();
        Vector3 spawnPosition = firepoint != null ? firepoint.transform.position : effectPayload.User.transform.position;
        
        // 2. Spawn the projectile
        GameObject projectileInstance = Object.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // 3. Apply scale
        if (projectileScale != 1f) projectileInstance.transform.localScale *= projectileScale;
        
        // 3. Pass the data to the projectile
        if (projectileInstance.TryGetComponent(out Projectile projectileComponent))
        {
            // Set up default straight line curve if null
            if (projectileCurve == null || projectileCurve.length == 0)
                projectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            
            // Setup the projectile with physics data and on hit effects list
            projectileComponent.Setup(
                effectPayload.TargetPosition, 
                projectileSpeed, 
                projectileLifetime, 
                projectileCurve, 
                projectileHeight, 
                effectPayload.User, 
                onHitEffects,
                maxEnemiesHit,
                hitOncePerTarget,
                destroyOnMaxHits
                );

            return true;
        }

        return false;
    }
    
}
