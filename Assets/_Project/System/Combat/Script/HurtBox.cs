using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class HurtBox : MonoBehaviour, IDamagable
{
    [SerializeField] private Health health;
    [SerializeField] private float invulnerabilityDuration = 5f;
    private float _invulnerabilityTimer;
    
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
        
        // Trigger visual effects ONCE here!
        if (_flashShader != null)
        {
            _flashShader.ApplyFlash();     // Start the white flash
            _flashShader.SetBlinking(true); // Start the invulnerability flicker
        }
    }
}