using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntityController {
    [Header("References")]
    [HideInInspector] public Mana mpComponent;
    [HideInInspector] public SpellControllerPlayer spellController;
    
    [Header("States")]
    [SerializeReference, SubclassSelector] public State<PlayerController> IdleState;
    [SerializeReference, SubclassSelector] public State<PlayerController> MoveState;
    [SerializeReference, SubclassSelector] public State<PlayerController> DashState;
    
    [Header("Movement Settings")]
    public float defaultDashTime = 10f;
    
    private bool _canMove = true;
    private Vector2 _rawInput;
    private Vector2 _rawMousePosition;
    private Camera _mainCam;
    
    // Public Data for States to Read
    public Vector2 MovementInput { get; private set; }
    public Vector3 WorldMousePosition { get; private set; }
    [HideInInspector] public bool dashInput;

    private void OnEnable() => EventBus.OnPlayerMovementToggleRequested += SetCanMove;
    private void OnDisable() => EventBus.OnPlayerMovementToggleRequested -= SetCanMove;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Cache references
        spellController = GetComponent<SpellControllerPlayer>();
        _mainCam = Camera.main;
        
        // Setup states
        IdleState?.Setup(this, StateMachine);
        MoveState?.Setup(this, StateMachine);
        DashState?.Setup(this, StateMachine);
    }

    protected virtual void Start()
    {
        // Default to the idle state
        StateMachine.SetupState(IdleState);
        
        mpComponent = PlayerStatsManager.Instance.ManaComponent; // Avoid race condition
    }

    protected override void Update()
    {
        base.Update();
        
        if (_mainCam != null)
        {
            // Calculate the distance from the camera to the ground plane (Z=0)
            float distanceToPlane = Mathf.Abs(_mainCam.transform.position.z);
        
            Vector3 mouseScreenPos = new Vector3(_rawMousePosition.x, _rawMousePosition.y, distanceToPlane);
        
            // Convert to world space
            Vector3 worldPos = _mainCam.ScreenToWorldPoint(mouseScreenPos);

            // Lock Z to 0 for 2D gameplay
            WorldMousePosition = new Vector3(worldPos.x, worldPos.y, 0f);
        }
    }
    
    // ---- Input Routing ----
    public void OnMove(InputAction.CallbackContext context)
    {
        // Always track input even if player is not moving
        _rawInput = context.ReadValue<Vector2>();
        
        // Only update movement input if player can move
        if (_canMove) 
            MovementInput = _rawInput;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) dashInput = true;
    }
    
    public void OnPoint(InputAction.CallbackContext context)
    {
        _rawMousePosition = context.ReadValue<Vector2>();
    }
    
    public void OnAttackM1(InputAction.CallbackContext context) => spellController.TryTriggerSpell(0, context);
    public void OnAttackQ(InputAction.CallbackContext context)  => spellController.TryTriggerSpell(1, context);
    public void OnAttackE(InputAction.CallbackContext context)  => spellController.TryTriggerSpell(2, context);
    public void OnAttackR(InputAction.CallbackContext context)  => spellController.TryTriggerSpell(3, context);
    public void OnAttackF(InputAction.CallbackContext context)  => spellController.TryTriggerSpell(4, context);
    
    // ---- Helper Methods ----
    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
        
        // Clear input for animator and state machine
        if (!_canMove)
        {
            MovementInput = Vector2.zero;
            EntityMover.SetMoveDirection(Vector2.zero);
            dashInput = false;
        }
        else
        {
            MovementInput = _rawInput;
        }
    }
}
