using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class HurtBox : MonoBehaviour, IDamagable
{
    [Header("References)")]
    [SerializeField] private Health health;
    
    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    private float _invulnerabilityTimer;

    [Header("FX")] 
    [SerializeField] private ParticleSystem hitFX;
    private ParticleSystem hitFXInstance;
    private FlashShader _flashShader; 

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        _flashShader = GetComponentInChildren<FlashShader>();
    }

    private void Update()
    {
        // Run the timer down
        if (_invulnerabilityTimer > 0)
        {
            _invulnerabilityTimer -= Time.deltaTime;
            
            // Stop blinking ONLY when the timer finishes crossing zero
            if (_invulnerabilityTimer <= 0)
            {
                if (_flashShader != null) _flashShader.SetBlinking(false);
            }
        }
    }

    public void TakeDamage(DamageData data)
    {
        // Ignore hits if we are already invulnerable
        if (_invulnerabilityTimer > 0) return;
        
        health.HpCurrent -= data.damageAmount;
        _invulnerabilityTimer = invulnerabilityDuration;
        
        // Handle knockback and impulse...
        if (TryGetComponent<EntityMover>(out EntityMover entityMover))
        {
            entityMover.ApplyKnockback(data.hitDirection, data.knockbackForce, data.knockbackDuration, data.knockbackHeight, data.source);
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();  
        }
        
        // Trigger shader effects
        if (_flashShader != null)
        {
            _flashShader.ApplyFlash();     // Start the white flash
            _flashShader.SetBlinking(true); // Start the invulnerability flicker
        }
        
        // Trigger hit fx
        SpawnHitFX(data.hitDirection, data.hitImpactPoint);
        
    }

    private void SpawnHitFX(Vector2 hitDirection, Vector2 hitImpactPoint)
    {
        float angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
        Quaternion fxRotation = Quaternion.Euler(0, 0, angle);
        
        // 3. Spawn the FX
        if (hitFX != null) 
            Instantiate(hitFX, hitImpactPoint, fxRotation);
        
    }
}