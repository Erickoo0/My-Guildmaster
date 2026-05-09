using System.Collections.Generic;
using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    [Header("Base Settings")] 
    public LayerMask victimLayer; // Layer to check for collisions
    public bool enableHitbox = true;
    [HideInInspector] public Collider2D entityCollider;

    [Header("Behavior Settings")] 
    [Tooltip("If true, the hitbox will only be triggered once per target")]
    public bool hitOncePerTarget = true;
    [Tooltip("If true, the hitbox will be disabled after the first hit")]
    public bool disableAfterFirstHit = true;
    private int maxEnemiesHitCount;
    
    
    protected DamageData damageData;
    protected bool isDamageDataAssigned = false;
    
    private readonly HashSet<IDamagable> targetsHit = new HashSet<IDamagable>();
    
    protected virtual void Awake()
    {
        entityCollider = GetComponent<Collider2D>();
    }

    public virtual void Setup(DamageData data)
    {
        damageData = data;
        isDamageDataAssigned = true;
        targetsHit.Clear();
        maxEnemiesHitCount = data.maxEnemiesHitCount;
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!enableHitbox || !isDamageDataAssigned) return;
        if (other.isTrigger) return;
        if (((1 << other.gameObject.layer) & victimLayer) == 0) return;
        if (other.gameObject == damageData.source) return;

        if (other.TryGetComponent<IDamagable>(out IDamagable victim))
        {
            // 1. Check if we already hit the target
            if (hitOncePerTarget && targetsHit.Contains(victim)) return;
            
            // 2. Calculate direction (Implemented by inherited classes)
            Vector2 direction = GetKnockbackDirection(other);
            
            // 3. Send the damage to the target
            DamageData finalData = damageData;
            finalData.hitDirection = direction;
            maxEnemiesHitCount--;

            if (SendDamage(finalData, other))
            {
                targetsHit.Add(victim);

                HandlePostHit(other);
                
                if (disableAfterFirstHit) enableHitbox = false;
            }
            
            // Destroy hitbox if max enemies hit count reached
            if (maxEnemiesHitCount <= 0) Destroy(gameObject);
        }
    }
    
    // All children must implement this method
    protected abstract Vector2 GetKnockbackDirection(Collider2D other);
    // All children can implement this method
    protected virtual void HandlePostHit(Collider2D other) { }
    
    protected bool SendDamage(DamageData data, Collider2D victimCollider)
    {
        if (!enableHitbox) return false;
        
        if (victimCollider.TryGetComponent<IDamagable>(out IDamagable victim))
        {
            victim.TakeDamage(data);
            return true; // Successfully hit an IDamagable
        }

        return false; // It was on the layer mask, but not an IDamagable
    }
}
