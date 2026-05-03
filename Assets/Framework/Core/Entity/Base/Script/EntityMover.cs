using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMover : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float moveSpeed = 5f;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDecay = 8f;
    private bool _isKnockedBack = false;
    private float _knockbackTimer;
    private float _totalKnockbackDuration;
    private float _knockbackHeight;
    private SpriteRenderer _spriteRenderer;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Collider2D _collider;
    
    public Vector2 MoveDirection => _moveDirection;
    public bool IsKnockedBack => _isKnockedBack;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (PauseManager.IsGamePaused)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (_isKnockedBack)
        {
            HandleKnockbackLoop();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction.normalized;
    }

    private void HandleKnockbackLoop()
    {
        // Horizontal Knockback decay
        _rigidbody.linearVelocity = Vector2.MoveTowards(_rigidbody.linearVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime * 10f);
        
        _knockbackTimer -= Time.fixedDeltaTime;
        
        // Vertical Knockback Decay
        if (_spriteRenderer != null && _knockbackHeight > 0)
        {
            // Calculate progress from 0 to 1
            float knockbackHeightProgress = 1f - (_knockbackTimer / _totalKnockbackDuration);
            
            // Calculate vertical offset via sine wave formula
            float yOffset = Mathf.Sin(knockbackHeightProgress * Mathf.PI) * _knockbackHeight;
            
            // Apply the offset
            _spriteRenderer.transform.localPosition = new Vector2(_spriteRenderer.transform.localPosition.x, yOffset);
        }
        
        // Snap to finish
        if (_knockbackTimer <= 0f || _rigidbody.linearVelocity.sqrMagnitude < 0.1f)
        {
            _isKnockedBack = false;
            _rigidbody.linearVelocity = Vector2.zero;
            if (_spriteRenderer != null)
                _spriteRenderer.transform.localPosition = Vector2.zero;
        }
    }

    private void HandleNormalMovement()
    {
        _rigidbody.linearVelocity = _moveDirection * moveSpeed;
    }
    

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float knockbackDuration, float knockbackHeight, GameObject source = null)
    {
        if (!gameObject.activeInHierarchy) return;

        _isKnockedBack = true;
        _knockbackTimer = knockbackDuration;
        _totalKnockbackDuration = knockbackDuration;
        _knockbackHeight = knockbackHeight;

        
        // Immediate velocity burst
        _rigidbody.linearVelocity = knockbackDirection * knockbackForce;

        // Temporarily ignore collision with the attacker
        if (source != null)
            StartCoroutine(TemporaryIgnoreCollision(source, knockbackDuration * 1.5f));
        
    }
    
    private IEnumerator TemporaryIgnoreCollision(GameObject source, float duration)
    {
        if (source.TryGetComponent<Collider2D>(out Collider2D sourceCol) && _collider != null)
        {
            Physics2D.IgnoreCollision(_collider, sourceCol, true);
            yield return new WaitForSeconds(duration);
            Physics2D.IgnoreCollision(_collider, sourceCol, false);
        }
    }
}
