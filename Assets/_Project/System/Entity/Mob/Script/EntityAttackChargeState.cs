using UnityEngine;

[System.Serializable]
public class EntityAttackChargeState : BaseAttackState
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeedMultiplier = 5.0f;
    [SerializeField] private float overshootDistance = 4.0f;

    private HitBox _chargeHitbox;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _entityCollider;
    private LayerMask _originalExcludeLayers;
    
    [Header("Timers & Tracking")]
    private float _chargeTimer;
    private Vector2 _chargeDirection;
    private bool _isCharging;
    private float _afterImageTimer;
    private float _afterImageInterval = 0.04f;

    public override void Setup(MobController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        _chargeHitbox = controller.GetComponentInChildren<HitBox>(true);
        if (_chargeHitbox == null)
        {
            Debug.LogError("No HitBox found on " + controller.gameObject.name);
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
        _chargeHitbox.enableHitbox = false;
        
        _entityCollider = controller.GetComponent<Collider2D>();
        _spriteRenderer = controller.gameObject.GetComponentInChildren<SpriteRenderer>(true);

        if (_entityCollider == null || _spriteRenderer == null)
        {
            Debug.LogError("No Collider2D or SpriteRenderer found on " + controller.gameObject.name + "");
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    }
    

    public override void Update()
    {
        base.Update();

        // Charge Phase Logic
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
                stateMachine.ChangeState(controller.IdleState);
        }
    }

    protected override void HandleAnimationEvent()
    {
        // Safety Check
        if (hasTriggered) return;
        hasTriggered = true;
        
        float chargeSpeed = controller.EntityMover.moveSpeed * chargeSpeedMultiplier;
        
        // 1. Freeze the Animator so it doesnt trigger animationEnd event and ending the attack before the timer
        if (controller.EntityAnimator != null)
            controller.EntityAnimator.animator.speed = 0f;
        
        // 1. Calculate Charge Vector and Timing
        TryUpdateAttackDirection();
        Vector2 chargeDirection = attackDirection;
        
        float distanceToTarget = Vector2.Distance(controller.transform.position, controller.currentTarget.position);
        float totalDistance = distanceToTarget + overshootDistance;
        _chargeTimer = totalDistance/chargeSpeed;
        
        
        // 2. Ignore collisions with victims during dash to avoid getting stuck
        _originalExcludeLayers = _entityCollider.excludeLayers;
        _entityCollider.excludeLayers |= _chargeHitbox.victimLayer;
        
        
        // 3. Pass the data and Turn on the hitbox
        _chargeHitbox.Setup(controller.gameObject, attackDataInstance.Effects, 999, true, false);
        _chargeHitbox.enableHitbox = true;
        
        // 4. Tell EntityMover to take over movement and pause AILerp
        controller.EntityMover.StartCharge(chargeDirection, chargeSpeed);

        _isCharging = true;
        _afterImageTimer = 0f;
    }
    

    
    public override void Exit()
    {
        _isCharging = false;

        controller.EntityMover.StopCharge();
        _chargeHitbox.enableHitbox = false;
        _entityCollider.excludeLayers = _originalExcludeLayers;
        
        base.Exit();
    }
    
    private void SpawnAfterImage()
    {
        GameObject entity = controller.gameObject;
        
        if (AfterImageManager.Instance != null && _spriteRenderer != null)
            AfterImageManager.Instance.SpawnAfterImage(_spriteRenderer.sprite, entity.transform.position, Color.red);
    }
}
