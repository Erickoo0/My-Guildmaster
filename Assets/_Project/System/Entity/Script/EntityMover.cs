using Pathfinding;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMover : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float moveSpeed = 5f;
    public Vector2 MoveDirection { get; private set; }
    public enum OverrideMovementState { None, KnockedBack, Recoiling }
    public OverrideMovementState currentOverrideState { get; private set; } = OverrideMovementState.None;

    [Header("Override State Settings")]
    private float _overrideTimer;
    private float _totalOverrideDuration;
    private float _knockbackHeight;
    private Vector2 _initialOverrideVelocity;
    
    [Header("References")]
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Collider2D _collider;
    
    public bool IsKnockedBack => currentOverrideState == OverrideMovementState.KnockedBack;
    
    private AILerp _aiLerp; // Add this reference

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _aiLerp = GetComponent<AILerp>();
    }

    private void FixedUpdate()
    {
        if (PauseManager.IsGamePaused)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        switch (currentOverrideState)
        {
        case OverrideMovementState.KnockedBack:
            if (_aiLerp != null) _aiLerp.canMove = false;
            HandleKnockbackLoop();
            break;
        case OverrideMovementState.Recoiling:
            if (_aiLerp != null) _aiLerp.canMove = false;
            HandleRecoiling();
            break;
        case OverrideMovementState.None: // If there is AILerp, use its own movement component
        default: // If there is no AILerp component, use EntityMover 
            if (_aiLerp == null) HandleNormalMovement();
            break;
        }
    }

    public void SetMoveDirection(Vector2 direction) => MoveDirection = direction.normalized;

    private void HandleKnockbackLoop()
    {
        _overrideTimer -= Time.fixedDeltaTime;
        
        // 1. Calculate progress from 0 to 1
        float knockbackProgress = Mathf.Clamp01(1f - (_overrideTimer / _totalOverrideDuration));
        
        // 2. Horizontal Decay logic
        _rigidbody.linearVelocity = Vector2.Lerp(_initialOverrideVelocity, Vector2.zero, knockbackProgress);
        
        // 3. Vertical Decay logic
        if (_spriteRenderer != null && _knockbackHeight > 0)
        {
            float yOffset = Mathf.Sin(knockbackProgress * Mathf.PI) * _knockbackHeight;
            _spriteRenderer.transform.localPosition = new Vector2(_spriteRenderer.transform.localPosition.x, yOffset);
        }
        
        // 4. Exit Logic
        if (_overrideTimer <= 0f) ClearOverride();
    }

    private void HandleRecoiling () 
    {
        _overrideTimer -= Time.fixedDeltaTime;
        
        // 1. Calculate progress from 0 to 1
        float recoilProgress = Mathf.Clamp01(1f - (_overrideTimer/_totalOverrideDuration));
        
        // 2. Horizontal Decay logic
        _rigidbody.linearVelocity = Vector2.Lerp(_initialOverrideVelocity, Vector2.zero, recoilProgress);
        
        // 3. Exit Logic
        if (_overrideTimer <= 0f) ClearOverride();
    }

    private void HandleNormalMovement() => _rigidbody.linearVelocity = MoveDirection * moveSpeed;

    private void ClearOverride()
    {
        currentOverrideState = OverrideMovementState.None;
        _rigidbody.linearVelocity = Vector2.zero;
        if (_spriteRenderer != null) _spriteRenderer.transform.localPosition = Vector2.zero;
        
        // Re-enable AILerp and update its internal position tracking
        if (_aiLerp != null)
        {
            _aiLerp.canMove = true;
            _aiLerp.Teleport(transform.position);
            _aiLerp.SearchPath();
        }
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float knockbackDuration, float knockbackHeight, GameObject source = null)
    {
        if (!gameObject.activeInHierarchy) return;

        currentOverrideState = OverrideMovementState.KnockedBack;
        _overrideTimer = knockbackDuration;
        _totalOverrideDuration = knockbackDuration;
        _knockbackHeight = knockbackHeight;
        
        // Store the starting velocity for lerp
        _initialOverrideVelocity = knockbackDirection * knockbackForce;
        _rigidbody.linearVelocity = _initialOverrideVelocity;

        // Temporarily ignore collision with the attacker
        if (source != null)
            StartCoroutine(TemporaryIgnoreCollision(source, knockbackDuration));
        
    }

    public void ApplyRecoil (Vector2 direction, float recoilForce = 8f, float recoilDuration = 0.1f) 
    {
        // Knockback takes priority over  recoil
        if (currentOverrideState == OverrideMovementState.KnockedBack) return;
        
        currentOverrideState = OverrideMovementState.Recoiling;
        _overrideTimer = recoilDuration;
        _totalOverrideDuration = recoilDuration;
        
        // Reverse the attack direction for recoil
        _initialOverrideVelocity = -direction.normalized * recoilForce;
        _rigidbody.linearVelocity = _initialOverrideVelocity;
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
