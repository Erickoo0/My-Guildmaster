using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseEntityController
{
    [Header("State References")]
    [SerializeReference, SubclassSelector] public State<PlayerController> IdleState;
    [SerializeReference, SubclassSelector] public State<PlayerController> MoveState;
    [SerializeReference, SubclassSelector] public State<PlayerController> DashState;
    [SerializeReference, SubclassSelector] public State<PlayerController> AttackState;
    
    [Header("Action Settings")]
    [SerializeField] private List<AttackData> attackLibrary;
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


    protected override void Awake()
    {
        base.Awake();
        _mainCam = Camera.main;
        
        IdleState?.Setup(this, StateMachine);
        MoveState?.Setup(this, StateMachine);
        DashState?.Setup(this, StateMachine);
        AttackState?.Setup(this, StateMachine);
    }

    protected virtual void Start()
    {
        // Default to the idle state
        StateMachine.SetupState(IdleState);
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
        
        // // Continuously update where the mouse is in the world so the Attack State can read it
        // if (_mainCam != null)
        // {
        //     float distanceToPlane = Mathf.Abs(_mainCam.transform.position.z);
        //     Vector3 mouseInput = new Vector3(_rawMousePosition.x, _rawMousePosition.y, distanceToPlane);
        //     WorldMousePosition = _mainCam.ScreenToWorldPoint(mouseInput);
        //     WorldMousePosition = new Vector3(WorldMousePosition.x, WorldMousePosition.y, 0f);
        // }
    }
    
    public void OnMouseClick(InputAction.CallbackContext context)
    { 
        // Only trigger the attack state if the button was just pressed, the cooldown is ready, 
        // and we aren't already attacking or dashing.
        if (context.performed && CheckActionCooldown() && 
            StateMachine.CurrentState != AttackState &&  
            StateMachine.CurrentState != DashState)
        {
            StateMachine.ChangeState(AttackState);
        }
    }
    
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
    
    public T GetAttackData<T>(string id) where T : AttackData
    {
        // Search the library for a piece of data that:
        // 1. Matches the ID string
        // 2. Is of the type (T) we are looking for
        return attackLibrary.OfType<T>().FirstOrDefault(data => data.attackID == id);
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
}
