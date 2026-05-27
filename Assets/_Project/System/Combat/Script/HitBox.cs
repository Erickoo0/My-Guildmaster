using System.Collections.Generic;
using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    [Header("Base Settings")] 
    public LayerMask victimLayer; // Layer to check for collisions
    public bool enableHitbox = true;
    
    [HideInInspector] public Collider2D entityCollider;
    
    private int _maxEnemiesHitCount;
    private bool _hitOncePerTarget;
    private bool _destroyOnMaxHits;
    
    protected DamageData baseDamageData;
    protected bool isDamageDataAssigned = false;
    
    private readonly HashSet<IDamagable> targetsHit = new HashSet<IDamagable>();
    
    protected virtual void Awake()
    {
        entityCollider = GetComponent<Collider2D>();
    }

    public virtual void Setup(DamageData data)
    {
        baseDamageData = data;
        isDamageDataAssigned = true;
        targetsHit.Clear();
        
        // Read the data from the DamageData
        _maxEnemiesHitCount = data.maxEnemiesHitCount;
        _hitOncePerTarget = data.hitOncePerTarget;
        _destroyOnMaxHits = data.destroyOnMaxHits;
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        // Safety Check
        if (!enableHitbox || !isDamageDataAssigned) return;
        if (other.isTrigger) return;
        
        // Filter out wrong layers and Source
        if (((1 << other.gameObject.layer) & victimLayer) == 0) return;
        if (other.gameObject == baseDamageData.source) return;

        if (other.TryGetComponent<IDamagable>(out IDamagable victim))
        {
            // 1. Check if we already hit the target
            if (_hitOncePerTarget && targetsHit.Contains(victim)) return;
            
            // 2. Calculate knockback direction (Implemented by inherited classes)
            CalculateImpactPhysics(other, out Vector2 direction, out Vector2 impactPoint);
            
            // 3. Send the damage to the target
            DamageData finalData = baseDamageData;
            finalData.hitDirection = direction;
            finalData.hitImpactPoint = impactPoint;
            
            if (SendDamage(finalData, other))
            {
                targetsHit.Add(victim);
                HandlePostHit(other);
                
                // If there is a enemies hit limit
                if (_maxEnemiesHitCount > 0)
                {
                    _maxEnemiesHitCount--;

                    if (_maxEnemiesHitCount <= 0)
                    {
                        enableHitbox = false;

                        if (_destroyOnMaxHits)
                            Destroy(gameObject);
                    }
                }
            }
        }
    }
    
    // All children must implement this method
    protected abstract void CalculateImpactPhysics(Collider2D other, out Vector2 knockbackDirection, out Vector2 impactPoint);
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
