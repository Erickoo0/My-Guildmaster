using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    [Header("Base Settings")] 
    public LayerMask VictimLayer; // Layer to check for collisions
    private bool _enableHitBox = false;
    public bool EnableHitBox
    {
        get => _enableHitBox;
        set
        {
            _enableHitBox = value;
            if (entityCollider != null)
                entityCollider.enabled = value;
        }
    }
    
    [HideInInspector] public Collider2D entityCollider;
    
    private int _maxEnemiesHitCount;
    private bool _hitOncePerTarget;
    private bool _destroyOnMaxHits;

    private GameObject spellSource;
    private List<Effect> onHitEffects;

    private HashSet<IDamagable> targetsHit;
    
    protected virtual void Awake() => entityCollider = GetComponent<Collider2D>();

    public virtual void Setup(GameObject user, [CanBeNull] List<Effect> effects, int maxHits, bool hitOnce, bool destroyOnMax, HashSet<IDamagable> inheritedTargets = null)
    {
        spellSource = user;
        onHitEffects = effects;
        
        // If memory of previous hits is passed down, use it. Otherwise, create a new HashSet
        targetsHit = inheritedTargets ?? new HashSet<IDamagable>();       
        
        // Read the data from the DamageData
        _maxEnemiesHitCount = maxHits;
        _hitOncePerTarget = hitOnce;
        _destroyOnMaxHits = destroyOnMax;
    }

    public void OnTriggerEnter2D(Collider2D other) => OnTriggerStay2D(other);
    public void OnTriggerStay2D(Collider2D other)
        {
            // 1. Safety Checks
            if (!EnableHitBox || other.isTrigger) return;
            if (((1 << other.gameObject.layer) & VictimLayer) == 0) return;
            if (other.transform.root.gameObject == spellSource.transform.root.gameObject) return;

            // 2. Identify what we hit
            IDamagable victim = other.GetComponentInParent<IDamagable>();
            bool isWall = other.gameObject.layer == LayerMask.NameToLayer("Collisions"); // Adjust string if your wall layer is named differently

            // If it's neither an enemy nor a wall, ignore it
            if (victim == null && !isWall) return;

            // 3. Check if we already hit this specific target
            if (victim != null && _hitOncePerTarget && targetsHit.Contains(victim)) return;
            
            // 4. Calculate knockback direction
            CalculateImpactPhysics(other, out Vector2 direction, out Vector2 impactPoint);
            
            // 5. Create effect payload
            EffectPayload effectPayload = new EffectPayload(
                spellSource,
                other.gameObject,
                other.transform.position,
                direction,
                impactPoint,
                targetsHit
            );
            
            // 6. Execute all skill effects
            bool anyEffectSucceeded = false;
            if (onHitEffects != null && onHitEffects.Count > 0)
            {
                foreach (Effect effect in onHitEffects)
                {
                    if (effect.Execute(effectPayload))
                        anyEffectSucceeded = true;
                }
            }

            // 7. Handle successful hits
            if (anyEffectSucceeded)
            {
                if (victim != null) targetsHit.Add(victim);
                HandlePostHit(other);
                
                // Wall Impact Logic
                if (isWall)
                {
                    if (_destroyOnMaxHits) Destroy(gameObject); 
                    return;
                }

                // Enemy Piercing Logic
                if (_maxEnemiesHitCount > 0)
                {
                    _maxEnemiesHitCount--;

                    if (_maxEnemiesHitCount <= 0)
                    {
                        EnableHitBox = false;
                        if (_destroyOnMaxHits) Destroy(gameObject);
                    }
                }
            }
        }
    
    // All children must implement this method
    protected abstract void CalculateImpactPhysics(Collider2D other, out Vector2 knockbackDirection, out Vector2 impactPoint);
    // All children can implement this method
    protected virtual void HandlePostHit(Collider2D other) { }
}
