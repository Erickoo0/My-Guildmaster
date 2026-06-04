// using UnityEngine;
//
// [System.Serializable]
// public class EntityChargeAttackState : BaseActionState
// {
//     [Header("References")] 
//     [SerializeField] private string attackID;
//     private SpellData _spellData;
//     private GameObject _spawnedAttackPrefab;
//     private HitBox _spawnedHitbox;
//     private Collider2D _entityCollider;
//     private LayerMask _originalExcludeLayers; // To restore after the charge
//     
//     [Header("Timers")]
//     private float _windUpTimer;
//     private float _chargeTimer;
//     private Vector2 _chargeDirection;
//     private float _originalSpeed;
//     private bool _isFinished;
//     private float _afterImageTimer;
//     private float _afterImageInterval = 0.04f;
//     
//     public override void Setup(MobController controller, StateMachine stateMachine)
//     {
//         base.Setup(controller, stateMachine);
//         
//         // Grab the attack data from the controllers attack library
//         _spellData = controller?.GetAttackData<SpellData>(attackID);
//         
//         if (_spellData == null) 
//             Debug.LogError($"ChargeSpellData not found in attack library for {controller.gameObject.name}");
//         
//         _entityCollider = controller.GetComponent<Collider2D>();
//     }
//     
//     public override void Enter()
//     {
//         // 1. Reset state flags
//         _isFinished = false;
//         _originalSpeed = controller.EntityMover.moveSpeed;
//         
//         // 2. Freeze and set timers
//         controller.EntityMover.SetMoveDirection(Vector2.zero);
//         _windUpTimer = _spellData.windUpDuration;
//     
//         // 3. Target Validation & Direction Logic
//         if (controller.currentTarget != null)
//         {
//             Vector2 direction = (Vector2)controller.currentTarget.position - (Vector2)controller.transform.position;
//             _chargeDirection = direction.normalized;
//     
//             float totalDistance = direction.magnitude + _spellData.overshootDistance;
//             float totalChargeSpeed = _originalSpeed * _spellData.chargeSpeedMultiplier;
//             _chargeTimer = totalDistance / totalChargeSpeed;
//             
//             controller.EntityAnimator.FaceDirection(_chargeDirection);
//         }
//         else 
//         {
//             _isFinished = true; 
//             return;
//         }
//     
//         // 4. Spawn the attack prefab (hitbox)
//         if (_spellData.spellPrefab != null)
//         {
//             // Spawn as child so it moves with the entity automatically
//             _spawnedAttackPrefab = Object.Instantiate(_spellData.spellPrefab, controller.transform);
//             _spawnedHitbox = _spawnedAttackPrefab.GetComponent<HitBox>();
//             
//             // Setup the hitbox
//             if (_spawnedHitbox != null)
//             {
//                 _spawnedHitbox.Setup(
//                     user: controller.gameObject, 
//                     effects: _spellData.spellEffects, 
//                     maxHits: _spellData.baseMaxEnemiesHit, 
//                     hitOnce: _spellData.hitOncePerTarget, 
//                     destroyOnMax: _spellData.destroyOnMaxHits
//                     );
//                 _spawnedHitbox.enableHitbox = false;
//             }
//         }
//     }
//     
//     public override void Update()
//     {
//         if (_isFinished)
//         {
//             stateMachine.ChangeState(controller.IdleState);
//             return;
//         }
//     
//         // Phase A: Windup
//         if (_windUpTimer > 0)
//         {
//             _windUpTimer -= Time.deltaTime;
//         }
//         // Phase B: Active Charge
//         else if (_chargeTimer > 0)
//         {
//             // Tell the entityCollider to ignore entity collision
//             if (_entityCollider != null)
//             {
//                 _originalExcludeLayers = _entityCollider.excludeLayers; // Save the original layers
//                 HitBox attackHitbox = _spellData.spellPrefab.GetComponent<HitBox>();
//                 if (attackHitbox != null)
//                     _entityCollider.excludeLayers |= attackHitbox.victimLayer;
//             }
//
//             _chargeTimer -= Time.deltaTime;
//             
//             controller.EntityMover.moveSpeed = _originalSpeed * _spellData.chargeSpeedMultiplier;
//             controller.EntityMover.SetMoveDirection(_chargeDirection);
//     
//             // Enable the hitbox
//             if (_spawnedHitbox != null)
//                 _spawnedHitbox.enableHitbox = true;
//             
//             // Create after images
//             if (_afterImageTimer <= 0)
//             {
//                 SpawnAfterImage();
//                 _afterImageTimer = _afterImageInterval;
//             }
//         
//             _afterImageTimer -= Time.deltaTime;
//             
//         }
//         // Phase C: Completion
//         else
//         {
//             _isFinished = true;
//         }
//         
//         if (_isFinished)
//             stateMachine.ChangeState(controller.IdleState);
//     }
//     
//     public override void Exit()
//     {
//         // Restore collision layers
//         if (_entityCollider != null)
//             _entityCollider.excludeLayers = _originalExcludeLayers;
//         
//         controller.EntityMover.moveSpeed = _originalSpeed;
//         controller.currentTarget = null;
//         controller.EntityMover.SetMoveDirection(Vector2.zero);
//         
//         controller.SetActionCooldown();
//         
//         if (_spawnedAttackPrefab != null)
//             Object.Destroy(_spawnedAttackPrefab.gameObject);
//         
//     }
//     
//     public override void PhysicsUpdate() { }
//     public override void HandleInput() { }
//     
//     private void SpawnAfterImage()
//     {
//         GameObject entity = controller.gameObject;
//         SpriteRenderer spriteRenderer = entity.GetComponentInChildren<SpriteRenderer>();
//         AfterImageManager.Instance.SpawnAfterImage(spriteRenderer.sprite, entity.transform.position, Color.softRed);
//     }
// }