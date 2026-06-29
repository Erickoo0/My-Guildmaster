using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectSpawnProjectile : Effect
{
    [Header("Projectile Prefab")]
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public float Scale { get; private set; } = 1f;
    
    [Header("Flight Physics")]
    [field: SerializeField] public float Speed { get; private set; } = 12f;
    [field: SerializeField] public  float Duration { get; private set; } = 3f;
    [field: SerializeField] public float ProjectileHeight { get; private set; } = 0f;
    [field: SerializeField] public AnimationCurve ProjectileCurve { get; private set; }

    [Header("Hitbox Settings")]
    [field: SerializeField] public int MaxEnemiesHit { get; private set; } = 1;
    [field: SerializeField] public  bool HitOncePerTarget { get; private set; } = true;
    [field: SerializeField] public bool DestroyOnMaxHits { get; private set; } = true;
    
    [Header("Impact EffectList List")]
    [SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();
    
    public override List<Effect> GetNestedEffects() => EffectsList;
    
    public override bool Execute(EffectPayload effectPayload)
    {
        if (Prefab == null) return false;
        
        // 1. Find the user's firepoint component if they have one
        FirePoint firepoint = effectPayload.User.GetComponentInChildren<FirePoint>();
        Vector3 spawnPosition = firepoint != null ? firepoint.transform.position : effectPayload.User.transform.position;
        
        // 2. Spawn the projectile
        GameObject projectileInstance = Object.Instantiate(Prefab, spawnPosition, Quaternion.identity);
        
        // 3. Apply scale
        if (Scale != 1f) projectileInstance.transform.localScale *= Scale;
        
        // 3. Pass the data to the projectile
        if (projectileInstance.TryGetComponent(out Projectile projectileComponent))
        {
            // Set up default straight line curve if null
            if (ProjectileCurve == null || ProjectileCurve.length == 0)
                ProjectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            
            // Setup the projectile with physics data and on hit effects list
            projectileComponent.Setup(
                effectPayload.TargetPosition, 
                Speed, 
                Duration, 
                ProjectileCurve, 
                ProjectileHeight, 
                effectPayload.User, 
                EffectsList,
                MaxEnemiesHit,
                HitOncePerTarget,
                DestroyOnMaxHits,
                Scale
                );

            return true;
        }

        return false;
    }

    public override Effect Clone()
    {
        // Clone the nested On Hit EffectsList list
        List<Effect> clonedOnHitEffects = new List<Effect>();
        if (EffectsList != null)
            foreach (Effect effect in EffectsList)
                if (effect != null) clonedOnHitEffects.Add(effect.Clone());

        return new EffectSpawnProjectile
        {
            Prefab = Prefab,
            Scale = Scale,
            Speed = Speed,
            Duration = Duration,
            ProjectileHeight = ProjectileHeight,
            ProjectileCurve = ProjectileCurve != null ? new AnimationCurve(this.ProjectileCurve.keys) : null,
            MaxEnemiesHit = MaxEnemiesHit,
            HitOncePerTarget = HitOncePerTarget,
            DestroyOnMaxHits = DestroyOnMaxHits,
            EffectsList = clonedOnHitEffects
        };
    }
}
