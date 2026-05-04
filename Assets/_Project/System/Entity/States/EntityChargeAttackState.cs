using UnityEngine;

[System.Serializable]
public class EntityChargeAttackState : BaseActionState
{
    [Header("References")] 
    [SerializeField] private string attackID;
    private ChargeAttackData _attackData;
    private GameObject _spawnedAttackPrefab;
    private HitBox _spawnedHitbox;
    private Collider2D _entityCollider;
    private LayerMask _originalExcludeLayers; // To restore after the charge
    
    [Header("Timers")]
    private float _windUpTimer;
    private float _chargeTimer;
    private Vector2 _chargeDirection;
    private float _originalSpeed;
    private bool _isFinished;
    
    public override void Setup(EntityController controller, StateMachine stateMachine)
    {
        base.Setup(controller, stateMachine);
        
        // Grab the attack data from the controllers attack library
        _attackData = controller?.GetAttackData<ChargeAttackData>(attackID);
        
        if (_attackData == null) 
            Debug.LogError($"ChargeAttackData not found in attack library for {controller.gameObject.name}");
        
        _entityCollider = controller.GetComponent<Collider2D>();
    }
    
    public override void Enter()
    {
        // 1. Reset state flags
        _isFinished = false;
        _originalSpeed = controller.EntityMover.moveSpeed;
        
        // 2. Freeze and set timers
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        _windUpTimer = _attackData.windUpDuration;
    
        // 3. Target Validation & Direction Logic
        if (controller.currentTarget != null)
        {
            Vector2 direction = (Vector2)controller.currentTarget.position - (Vector2)controller.transform.position;
            _chargeDirection = direction.normalized;
    
            float totalDistance = direction.magnitude + _attackData.overshootDistance;
            float totalChargeSpeed = _originalSpeed * _attackData.chargeSpeedMultiplier;
            _chargeTimer = totalDistance / totalChargeSpeed;
            
            controller.EntityAnimator.FaceDirection(_chargeDirection);
        }
        else 
        {
            _isFinished = true; 
            return;
        }
    
        // 4. Spawn the attack prefab (hitbox)
        if (_attackData.attackPrefab != null)
        {
            // Spawn as child so it moves with the entity automatically
            _spawnedAttackPrefab = Object.Instantiate(_attackData.attackPrefab, controller.transform);
            _spawnedHitbox = _spawnedAttackPrefab.GetComponent<HitBox>();
            
            // Setup the hitbox
            if (_spawnedHitbox != null)
            {
                _spawnedHitbox.enableHitbox = false;
                
                // Create a local instance of the damage data and pass the source
                DamageData damageData = _attackData.damageData;
                damageData.source = controller.gameObject;
                
                _spawnedHitbox.Setup(damageData);
            }
        }
    }
    
    public override void Update()
    {
        if (_isFinished)
        {
            stateMachine.ChangeState(controller.IdleState);
            return;
        }
    
        // Phase A: Windup
        if (_windUpTimer > 0)
        {
            _windUpTimer -= Time.deltaTime;
        }
        // Phase B: Active Charge
        else if (_chargeTimer > 0)
        {
            // Tell the entityCollider to ignore entity collision
            if (_entityCollider != null)
            {
                _originalExcludeLayers = _entityCollider.excludeLayers; // Save the original layers
                HitBox attackHitbox = _attackData.attackPrefab.GetComponent<HitBox>();
                if (attackHitbox != null)
                    _entityCollider.excludeLayers |= attackHitbox.victimLayer;
            }

            _chargeTimer -= Time.deltaTime;
            
            controller.EntityMover.moveSpeed = _originalSpeed * _attackData.chargeSpeedMultiplier;
            controller.EntityMover.SetMoveDirection(_chargeDirection);
    
            // Enable the hitbox
            if (_spawnedHitbox != null)
                _spawnedHitbox.enableHitbox = true;
            
        }
        // Phase C: Completion
        else
        {
            _isFinished = true;
        }
        
        if (_isFinished)
            stateMachine.ChangeState(controller.IdleState);
    }
    
    public override void Exit()
    {
        // Restore collision layers
        if (_entityCollider != null)
            _entityCollider.excludeLayers = _originalExcludeLayers;
        
        controller.EntityMover.moveSpeed = _originalSpeed;
        controller.currentTarget = null;
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        
        controller.SetActionCooldown();
        
        if (_spawnedAttackPrefab != null)
            Object.Destroy(_spawnedAttackPrefab.gameObject);
        
    }
    
    public override void PhysicsUpdate() { }
    public override void HandleInput() { }
}