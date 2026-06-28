using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectSpawnExplosion : Effect
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private float _lifeTime = 0.5f;
    [SerializeField] private float _scale = 1f;
    
    [Header("Explosion Impact Settings")]
    [SerializeReference, SubclassSelector] public List<Effect> EffectsList = new List<Effect>();

    public override List<Effect> GetNestedEffects() => EffectsList;
    
    public override bool Execute(EffectPayload effectPayload)
    {
        if (_prefab == null) return false;
        
        // 1. Determine spawn location
        Vector3 spawnPosition = effectPayload.HitImpactPoint != Vector2.zero ? (Vector3)effectPayload.HitImpactPoint : effectPayload.TargetPosition;
        
        // 2. Spawn the explosion
        GameObject explosionInstance = Object.Instantiate(_prefab, spawnPosition, Quaternion.identity);
        Object.Destroy(explosionInstance, _lifeTime);
        
        // 3. Pass the data
        if (explosionInstance.TryGetComponent(out HitBoxAOE hitBox))
        {
            hitBox.Setup(
                user: effectPayload.User,
                effects: EffectsList,
                maxHits: 999,                              
                hitOnce: true,                             
                destroyOnMax: false,                       
                inheritedTargets: effectPayload.HitTargets // Pass the memory chain!
                );
            
            hitBox.EnableHitBox = true;
            return true;
        }
        
        return false;
    }

    public override Effect Clone()
    {
        List<Effect> clonedExplosionEffects = new List<Effect>();
        if (EffectsList != null)
            foreach (Effect effect in EffectsList)
                if (effect != null)
                    clonedExplosionEffects.Add(effect.Clone());

        return new EffectSpawnExplosion
        {
            _prefab = _prefab,
            _lifeTime = _lifeTime,
            _scale = _scale,
            EffectsList = clonedExplosionEffects
        };
    }
}
