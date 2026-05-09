using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMover : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float moveSpeed = 5f;
    
    [Header("Knockback Settings")]
    private bool _isKnockedBack = false;
    private float _knockbackTimer;
    private float _totalKnockbackDuration;
    private float _knockbackHeight;
    private Vector2 _initialKnockbackVelocity;
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
        _knockbackTimer -= Time.fixedDeltaTime;
        
        // 1. Calculate progress from 0 to 1
        float knockbackProgress = Mathf.Clamp01(1f - (_knockbackTimer / _totalKnockbackDuration));
        
        // 2. Horizontal Decay logic
        _rigidbody.linearVelocity = Vector2.Lerp(_initialKnockbackVelocity, Vector2.zero, knockbackProgress);
        
        // 3. Vertical Decay logic
        if (_spriteRenderer != null && _knockbackHeight > 0)
        {
            float yOffset = Mathf.Sin(knockbackProgress * Mathf.PI) * _knockbackHeight;
            _spriteRenderer.transform.localPosition = new Vector2(_spriteRenderer.transform.localPosition.x, yOffset);
        }
        
        // 4. Exit Logic
        if (_knockbackTimer <= 0f)
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

        
        // Store the starting velocity for lerp
        _initialKnockbackVelocity = knockbackDirection * knockbackForce;
        _rigidbody.linearVelocity = _initialKnockbackVelocity;

        // Temporarily ignore collision with the attacker
        if (source != null)
            StartCoroutine(TemporaryIgnoreCollision(source, knockbackDuration));
        
    }
    
    private IEnumerator TemporaryIgnoreCollision(GameObject source, float knockbackDuration)
    {
        if (source.TryGetComponent<Collider2D>(out Collider2D sourceCol) && _collider != null)
        {
            Physics2D.IgnoreCollision(_collider, sourceCol, true);
            yield return new WaitForSeconds(knockbackDuration);
            Physics2D.IgnoreCollision(_collider, sourceCol, false);
        }
    }
}
