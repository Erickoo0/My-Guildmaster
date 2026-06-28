using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectSpawnProjectile : Effect
{
    [Header("Projectile Prefab")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private float _scale = 1f;
    
    [Header("Flight Physics")]
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private float _projectileHeight = 0f;
    [SerializeField] private AnimationCurve _projectileCurve;

    [Header("Hitbox Settings")]
    [SerializeField] private int _maxEnemiesHit = 1;
    [SerializeField] private bool _hitOncePerTarget = true;
    [SerializeField] private bool _destroyOnMaxHits = true;
    
    [Header("Impact EffectList List")]
    [SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();
    
    public override List<Effect> GetNestedEffects() => EffectsList;
    
    public override bool Execute(EffectPayload effectPayload)
    {
        if (_prefab == null) return false;
        
        // 1. Find the user's firepoint component if they have one
        FirePoint firepoint = effectPayload.User.GetComponentInChildren<FirePoint>();
        Vector3 spawnPosition = firepoint != null ? firepoint.transform.position : effectPayload.User.transform.position;
        
        // 2. Spawn the projectile
        GameObject projectileInstance = Object.Instantiate(_prefab, spawnPosition, Quaternion.identity);
        
        // 3. Apply scale
        if (_scale != 1f) projectileInstance.transform.localScale *= _scale;
        
        // 3. Pass the data to the projectile
        if (projectileInstance.TryGetComponent(out Projectile projectileComponent))
        {
            // Set up default straight line curve if null
            if (_projectileCurve == null || _projectileCurve.length == 0)
                _projectileCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            
            // Setup the projectile with physics data and on hit effects list
            projectileComponent.Setup(
                effectPayload.TargetPosition, 
                _speed, 
                _lifeTime, 
                _projectileCurve, 
                _projectileHeight, 
                effectPayload.User, 
                EffectsList,
                _maxEnemiesHit,
                _hitOncePerTarget,
                _destroyOnMaxHits,
                _scale
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
            _prefab = _prefab,
            _scale = _scale,
            _speed = _speed,
            _lifeTime = _lifeTime,
            _projectileHeight = _projectileHeight,
            _projectileCurve = _projectileCurve != null ? new AnimationCurve(this._projectileCurve.keys) : null,
            _maxEnemiesHit = _maxEnemiesHit,
            _hitOncePerTarget = _hitOncePerTarget,
            _destroyOnMaxHits = _destroyOnMaxHits,
            EffectsList = clonedOnHitEffects
        };
    }
}
