using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class ControllerPlayer : ControllerBase
{

	[Header("References")]
	[HideInInspector] public SkillControllerPlayer SkillController;
	[HideInInspector] public Mana MpComponent;


	[Header("States")]
	[SerializeReference, SubclassSelector] public State<ControllerPlayer> IdleState;
	[SerializeReference, SubclassSelector] public State<ControllerPlayer> MoveState;
	[SerializeReference, SubclassSelector] public State<ControllerPlayer> DashState;

	[Header("Movement Settings")]
	public float DefaultDashTime = 10f;
	[HideInInspector] public bool DashInput;

	private bool _canMove = true;
	private Camera _mainCam;
	private Vector2 _rawInput;
	private Vector2 _rawMousePosition;
	public IStatProvider StatProvider { get; private set; }
	public CinemachineImpulseSource CinemachineImpulseSource { get; private set; }

	// Public Data for States to Read
	public Vector2 MovementInput { get; private set; }
	public Vector3 WorldMousePosition { get; private set; }

	protected override void Awake()
	{
		base.Awake();

		// Cache references
		SkillController = GetComponent<SkillControllerPlayer>();
		StatProvider = GetComponent<IStatProvider>();
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

		MpComponent = PlayerStatsManager.Instance.ManaComponent; // Avoid race condition
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

	private void OnEnable() => EventBus.OnPlayerMovementToggleRequested += SetCanMove;
	private void OnDisable() => EventBus.OnPlayerMovementToggleRequested -= SetCanMove;

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
		if (context.performed) DashInput = true;
	}

	public void OnPoint(InputAction.CallbackContext context)
	{
		_rawMousePosition = context.ReadValue<Vector2>();
	}

	public void OnAttackM1(InputAction.CallbackContext context) => SkillController.TryTriggerSkill(0, context);
	public void OnAttackQ(InputAction.CallbackContext context) => SkillController.TryTriggerSkill(1, context);
	public void OnAttackE(InputAction.CallbackContext context) => SkillController.TryTriggerSkill(2, context);
	public void OnAttackR(InputAction.CallbackContext context) => SkillController.TryTriggerSkill(3, context);
	public void OnAttackF(InputAction.CallbackContext context) => SkillController.TryTriggerSkill(4, context);

	// ---- Helper Methods ----
	public void SetCanMove(bool canMove)
	{
		_canMove = canMove;

		// Clear input for animator and state machine
		if (!_canMove)
		{
			MovementInput = Vector2.zero;
			EntityMover.SetMoveDirection(Vector2.zero);
			DashInput = false;
		} else
		{
			MovementInput = _rawInput;
		}
	}
}
