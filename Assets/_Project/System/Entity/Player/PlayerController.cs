using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntityController {
    [Header("References")]
    [HideInInspector] public Mana mpComponent;
    [SerializeReference, SubclassSelector] public State<PlayerController> IdleState;
    [SerializeReference, SubclassSelector] public State<PlayerController> MoveState;
    [SerializeReference, SubclassSelector] public State<PlayerController> DashState;
    [HideInInspector] public FirePoint _firePoint;


    [Header("Spell Settings")]
    public SpellDataDatabase globalSpellDatabase;
    [Tooltip("Slot 0: M1, Slot 1: Q, Slot 2: E, Slot 3: R, Slot 4: F")]
    [SerializeReference, SubclassSelector] public List<BasePlayerSpellState> SpellSlots = new List<BasePlayerSpellState>();
    private bool[] _spellKeyHeld = new bool[5];
    
    [Header("Action Settings")]
    [field: SerializeField] public float ActionCooldown  { get; private set; } = 1f;
    private float _lastActionTime;

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
        _mainCam = Camera.main;
        _firePoint = GetComponentInChildren<FirePoint>();
        
        IdleState?.Setup(this, StateMachine);
        MoveState?.Setup(this, StateMachine);
        DashState?.Setup(this, StateMachine);

        foreach (State<PlayerController> spell in SpellSlots)
        {
            spell?.Setup(this, StateMachine);
        }
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

    public void TryTriggerAbility(int slotIndex, InputAction.CallbackContext context)
    {
        // 1. Safety Check
        if (SpellSlots == null || slotIndex < 0 || slotIndex >= SpellSlots.Count || SpellSlots[slotIndex] == null)
            return;
        
        // 2. Track key input state 
        if (context.started || context.performed)
            _spellKeyHeld[slotIndex] = true;
        else if (context.canceled)
            _spellKeyHeld[slotIndex] = false;
        
        // 3. Only proceed with the actual cast on the 'performed' phase
        if (!context.performed)
            return;
        
        BasePlayerSpellState intendedSpell = SpellSlots[slotIndex];

        // 4. State & Cooldown validation
        if (!CheckActionCooldown() || StateMachine.CurrentState == DashState)
            return;
        
        // 5. Prevent interrupting an ongoing spell with another spell
        if (SpellSlots.Contains(StateMachine.CurrentState))
            return;
        
        // 6. Mana validation 
        if (mpComponent == null || !mpComponent.HasEnoughMp(intendedSpell.MpCost))
            return;
        
        // 7. Assign slot index and change state
        intendedSpell.CurrentSlotIndex = slotIndex;
        StateMachine.ChangeState(intendedSpell);
    }
    
    public void OnAttackM1(InputAction.CallbackContext context) => TryTriggerAbility(0, context);
    public void OnAttackQ(InputAction.CallbackContext context)  => TryTriggerAbility(1, context);
    public void OnAttackE(InputAction.CallbackContext context)  => TryTriggerAbility(2, context);
    public void OnAttackR(InputAction.CallbackContext context)  => TryTriggerAbility(3, context);
    public void OnAttackF(InputAction.CallbackContext context)  => TryTriggerAbility(4, context);
    
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
    
    //---- Action Methods -----
    public bool CheckActionCooldown() 
    {
        return Time.time >= _lastActionTime + ActionCooldown;
    }
    
    // A method to reset the timer (called when the action finishes)
    public void SetActionCooldown()
    {
        _lastActionTime = Time.time;
    }

    public bool IsSpellKeyHeld(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _spellKeyHeld.Length) return false;
        return _spellKeyHeld[slotIndex];
    }
}
