using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class HurtBox : MonoBehaviour, IDamagable
{
    [Tooltip("Leave empty if parent object")]
    [SerializeField] private Health health;
    //private Animator animator;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
    }

    public void TakeDamage(DamageData data)
    {
        health.HpCurrent -= data.damageAmount;
        
        //animator.SetTrigger("Hurt");
        
        if (TryGetComponent<EntityMover>(out EntityMover entityMover))
        {
            entityMover.ApplyKnockback(data.hitDirection, data.knockbackForce, data.knockbackDuration, data.knockbackHeight, data.source);
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();  
        }
        
        // Apply flash shader
        FlashShader flashShader = GetComponentInChildren<FlashShader>();
        if (flashShader != null)
        {
            Debug.Log("Flash Shader Found");
            flashShader.ApplyFlash();

        }
    }
}
