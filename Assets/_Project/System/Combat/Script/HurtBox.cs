using Unity.Cinemachine;
using UnityEngine;
[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class HurtBox : MonoBehaviour, IDamagable
{
	[Header("References)")]
	[Tooltip("If null, will look for a Health component on the same GameObject")]
	[SerializeField] private Health _health;

	[Header("FX")]
	private FlashShader _flashShader;
	private float _invulnerabilityTimer;

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
		// 1. Ignore hits if we are already invulnerable unless its a bonus hit
		if (_invulnerabilityTimer > 0 && !damageData.IsBonusHit) return;

		// 2. Apply damage
		float finalDamage = DamageCalculator.CalculateFinalDamage(
			damageData.Amount,
			damageData.Type,
			damageData.Source,
			gameObject
			);

		_health.HpCurrent -= finalDamage;

		// 3. Only start invulnerability from non-bonus hits
		if (!damageData.IsBonusHit)
		{
			_invulnerabilityTimer = damageData.InvulnerableTimer;

			// Trigger shader effects
			if (_flashShader != null)
			{
				_flashShader.ApplyFlash();      // Start the white flash
				_flashShader.SetBlinking(true); // Start the invulnerability flicker
			}

			// Handle Squash and Stretch
			if (TryGetComponent<SquashAndStretch>(out SquashAndStretch squishAndSquashEffect))
				squishAndSquashEffect.SquishAndSquash();
		}

		// 4. Handle knockback and impulse...
		if (TryGetComponent<EntityMover>(out EntityMover entityMover))
		{
			entityMover.ApplyKnockback(damageData.Direction, damageData.KnockbackForce, damageData.KnockbackDuration, damageData.KnockbackHeight, damageData.Source);
			GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		}
	}
}
