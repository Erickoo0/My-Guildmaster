using UnityEngine;

// A Data Hub that provides the API for states
[RequireComponent(typeof(EntityMover), typeof(StateMachine))]
public abstract class ControllerBase : MonoBehaviour, ITargetable
{
	[Header("ID")]
	[SerializeField] private string entityID;

	[Header("References")]
	protected internal StateMachine StateMachine { get; private set; }
	public EntityMover EntityMover { get; private set; }
	public EntityAnimator EntityAnimator { get; private set; }

	protected virtual void Awake()
	{
		// Get all the components
		StateMachine = GetComponent<StateMachine>();
		EntityMover = GetComponent<EntityMover>();
		EntityAnimator = GetComponentInChildren<EntityAnimator>();
	}

	protected virtual void Update()
	{
		if (PauseManager.IsGamePaused) return;

		StateMachine.UpdateState(); // Link the State Machine Update method to Unity Update
	}

	protected virtual void FixedUpdate()
	{
		if (PauseManager.IsGamePaused) return;

		StateMachine.FixedUpdateState(); // Link the State Machine Fixed Update method to Unity FixedUpdate
	}
	public string GetTargetID() => entityID;
}
