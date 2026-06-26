using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class HurtBox : MonoBehaviour, IDamagable
{
    [Header("References)")]
    [Tooltip("If null, will look for a Health component on the same GameObject")]
    [SerializeField] private Health _health;
    
    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 0.5f;
    private float _invulnerabilityTimer;

    [Header("FX")] 
    private FlashShader _flashShader; 

    private void Awake()
    {
        if (_health == null) _health = GetComponent<Health>();
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

    public void TakeDamage(DamageData damageData)
    {
        // Ignore hits if we are already invulnerable
        if (_invulnerabilityTimer > 0) return;
        
        _health.HpCurrent -= damageData.Amount;
        _invulnerabilityTimer = _invulnerabilityDuration;
        
        // Handle knockback and impulse...
        if (TryGetComponent<EntityMover>(out EntityMover entityMover))
        {
            entityMover.ApplyKnockback(damageData.Direction, damageData.KnockbackForce, damageData.KnockbackDuration, damageData.KnockbackHeight, damageData.Source);
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();  
        }
        
        // Handle Squash and Stretch
        if (TryGetComponent<SquashAndStretch>(out SquashAndStretch squishAndSquashEffect)) 
            squishAndSquashEffect.SquishAndSquash();
        
        
        // Trigger shader effects
        if (_flashShader != null)
        {
            _flashShader.ApplyFlash();     // Start the white flash
            _flashShader.SetBlinking(true); // Start the invulnerability flicker
        }
    }

}