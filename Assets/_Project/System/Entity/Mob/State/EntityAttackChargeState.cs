using UnityEngine;

[System.Serializable]
public class EntityAttackChargeState : BaseAttackState
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeedMultiplier = 2.5f;
    [SerializeField] private float overshootDistance = 1.0f;

    private HitBox _chargeHitbox;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _entityCollider;
    private LayerMask _originalExcludeLayers;
    
    [Header("Timers & Tracking")]
    private float _chargeTimer;
    private Vector2 _chargeDirection;
    private float _originalSpeed;
    private bool _isCharging;
    private float _afterImageTimer;
    private float _afterImageInterval = 0.04f;

    public override void Enter()
    {
        base.Enter();
        
        _isCharging = true;
        _originalSpeed = controller.EntityMover.moveSpeed;
        
        // Face the target
        if (controller.currentTarget != null)
        {
            Vector2 aimDirection = (controller.currentTarget.position - controller.transform.position).normalized;
            controller.EntityAnimator.FaceDirection(aimDirection);
        }
        
        _spriteRenderer = controller.gameObject.GetComponentInChildren<SpriteRenderer>(true);
    }

    public override void Update()
    {
        base.Update();

        if (_isCharging)
        {
            _chargeTimer -= Time.deltaTime;
            
            // Handle AfterImages
            if (_afterImageTimer <= 0)
            {
                SpawnAfterImage();
                _afterImageTimer = _afterImageInterval;
            }
            _afterImageTimer -= Time.deltaTime;
            
            // End the charge phase when the timer is over
            if (_chargeTimer <= 0)
            {
                StopCharge();
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Lock in movement direction during the dash
        if (_isCharging)
            controller.EntityMover.SetMoveDirection(_chargeDirection);    
    }

    protected override void HandleAnimationEvent()
    {
        // Safety Check
        if (hasTriggered) return;
        hasTriggered = true;
        
        // 1. Calculate Charge Vector and Timing
        if (controller.currentTarget != null)
        {
            Vector2 direction = controller.currentTarget.position - controller.transform.position;
            _chargeDirection = direction.normalized;
            
            float totalDistance = direction.magnitude + overshootDistance;
            float totalChargeSpeed = _originalSpeed * chargeSpeedMultiplier;
            _chargeTimer = totalDistance / totalChargeSpeed;
        } 
        
        // 2. Ignore collisions with victims during dash to avoid getting stuck
        if (_entityCollider != null && _chargeHitbox != null)
        {
            _originalExcludeLayers = _entityCollider.excludeLayers;
            _entityCollider.excludeLayers |= _chargeHitbox.victimLayer;
        }
        
        // 3. Apply speed and turn on hitbox
        controller.EntityMover.moveSpeed = _originalSpeed * chargeSpeedMultiplier;
        if (_chargeHitbox != null && attackData != null)
        {
            _chargeHitbox.Setup(controller.gameObject, attackData.spellEffects, 999, true, false);
            _chargeHitbox.enableHitbox = true;
        }

        _isCharging = true;
        _afterImageTimer = 0f;
    }
    
    private void StopCharge()
    {
        if (!_isCharging) return;
        _isCharging = false;
        
        // Reset speed, stop moving, and turn off hitbox
        controller.EntityMover.moveSpeed = _originalSpeed;
        controller.EntityMover.SetMoveDirection(Vector2.zero);

        if (_chargeHitbox != null) _chargeHitbox.enableHitbox = false;

        // Restore collision layers
        if (_entityCollider != null)
            _entityCollider.excludeLayers = _originalExcludeLayers;
    }
    
    public override void Exit()
    {
        StopCharge(); // Safety net in case mob gets stunned/killed mid-charge
        base.Exit();
    }
    
    private void SpawnAfterImage()
    {
        GameObject entity = controller.gameObject;
        
        if (AfterImageManager.Instance != null && _spriteRenderer != null)
            AfterImageManager.Instance.SpawnAfterImage(_spriteRenderer.sprite, entity.transform.position, Color.red);
    }
}
