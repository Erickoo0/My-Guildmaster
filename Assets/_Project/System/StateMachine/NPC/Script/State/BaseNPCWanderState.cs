using System.Collections.Generic;
using UnityEngine;
public abstract class BaseNPCWanderState : State<NPCController>
{
	protected readonly float _positionCheckInterval = 0.5f;
	protected readonly float _stuckThreshold = 0.1f;
	protected readonly float _stuckTimerMax = 5f;
	protected int _currentPOIIndex = 0;
	protected bool _IsMovingToTransit = false;
	protected Vector2 _lastPosition;

	[Header("Location Settings")]
	protected List<PointOfInterest> _poiList = new List<PointOfInterest>();
	protected float _positionCheckTimer;
	protected PointOfInterest _selectedPOI;

	[Header("Anti-Stuck Variables")]
	protected float _stuckTimer;
	public string stateName;

	[Header("Pathing Logic")]
	[SerializeField] protected bool walkInSequence = false;

	// 1. Child classes must provide the correct POI IDs
	protected virtual List<string> GetPOITargetIDs() => new List<string>();

	public override void Enter()
	{
		// 1.  Tell the AI to start calculating paths again
		if (controller.aiLerp != null)
		{
			controller.aiLerp.canSearch = true;
			controller.aiLerp.canMove = true;
		}

		// 2. Reset state flags
		_stuckTimer = 0f;
		_positionCheckTimer = _positionCheckInterval;
		_lastPosition = controller.transform.position;

		// 3. Ask the POI Registry for the POI objects
		_poiList = POIRegistry.GetPOIByIDs(GetPOITargetIDs());

		// 4. Set Destination
		if (_poiList.Count > 0)
			SetNewDestination();
	}

	public override void Update()
	{
		// Safety Check
		if (controller.aiLerp == null) return;

		// 1. Pause state logic if knocked back
		if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;

		// 2. Destination Arrival logic
		if (!controller.aiLerp.pathPending && controller.aiLerp.reachedEndOfPath)
		{
			OnReachedDestination();
			return;
		}

		// 3. Apply Animation
		if (controller.EntityAnimator != null)
			controller.EntityAnimator.SetMoveAnimation(controller.aiLerp.velocity);

		CheckForStuck();
	}

	protected virtual void SetNewDestination()
	{
		if (_poiList == null || _poiList.Count == 0) return;

		PointOfInterest ultimateDestination;

		// 1. Pick a destination
		if (walkInSequence)
		{
			// Safety Check
			if (_currentPOIIndex >= _poiList.Count) _currentPOIIndex = 0;
			// Set the destination to the current POI index
			ultimateDestination = _poiList[_currentPOIIndex];
			_currentPOIIndex++;
		} else // If not walk in sequence, pick a random destination
		{
			ultimateDestination = _poiList[Random.Range(0, _poiList.Count)];
		}

		// 2. Are we in the correct location?
		if (controller.currentLocation != ultimateDestination.Location)
		{
			// 3. We are in the wrong location. Ask the GPS for the correct door to take.
			PointOfInterest transitNode = LocationRouter.GetNextTransitNode(controller.currentLocation, ultimateDestination.Location);

			if (transitNode != null)
			{
				_IsMovingToTransit = true;
				_selectedPOI = transitNode;
			} else
			{
				Debug.LogError($"[{controller.gameObject.name}] is stuck! No route from {controller.currentLocation} to {ultimateDestination.Location}");
				return;
			}
		} else
		{
			// We are in the correct room. Walk straight to the destination.
			_IsMovingToTransit = false;
			_selectedPOI = ultimateDestination;
		}

		// 4. Send the data to the A* Pathfinding
		controller.aiLerp.destination = _selectedPOI.transform.position;
		controller.aiLerp.SearchPath();
	}

	private void CheckForStuck()
	{
		// Tick down interval timer
		_positionCheckTimer -= Time.deltaTime;

		if (_positionCheckTimer <= 0f)
		{
			// Check if the entity moved less than the threshold over the LAST 0.5 SECONDS
			if (Vector2.Distance(_lastPosition, controller.transform.position) < _stuckThreshold)
			{
				_stuckTimer += _positionCheckInterval;
			} else
			{
				_stuckTimer = 0;
			}

			if (_stuckTimer > _stuckTimerMax)
			{
				Debug.Log("SkillControllerEntity stuck! Resetting path.");
				_stuckTimer = 0;
				SetNewDestination();
			}

			// Reset the interval variables
			_lastPosition = controller.transform.position;
			_positionCheckTimer = _positionCheckInterval;
		}
	}

	protected virtual void OnReachedDestination()
	{
		// 1. Check if POI is a teleporter
		if (!string.IsNullOrEmpty(_selectedPOI.TeleportPOI))
		{
			// 2. Ask the POI Registry for the associated GameObject of the string
			PointOfInterest teleportTarget = POIRegistry.GetPOIByID(_selectedPOI.TeleportPOI);

			// 3. Ensure the registry found the associated GameObject
			if (teleportTarget != null)
			{
				// Teleport the entity to the teleportTarget
				controller.aiLerp.Teleport(teleportTarget.transform.position); // Teleport to the teleportTarget POI

				// Clear lingering physics velocity to be safe
				if (controller._rigidbody2D != null) controller._rigidbody2D.linearVelocity = Vector2.zero;

				controller.currentLocation = teleportTarget.Location;
				controller.EntityAnimator.FaceDirection(teleportTarget.lookDirection);
			} else
				Debug.LogWarning($"[{controller.gameObject.name}] Teleport failed! Could not find POI with ID: '{_selectedPOI.TeleportPOI}' in the POIRegistry.");
		} else // If not a teleporter, normal arrival
		{
			controller.currentLocation = _selectedPOI.Location;
			controller.EntityAnimator.FaceDirection(_selectedPOI.lookDirection);
		}

		// Check if we just arrived at a transit POI, if so, skip idle, and keep moving
		if (_IsMovingToTransit)
		{
			_IsMovingToTransit = false;

			// Cast the state and trigger the skip idle time
			if (controller.IdleState is NPCIdleState idleState)
				idleState.SkipNextIdle();
		}

		stateMachine.ChangeState(controller.IdleState);
	}

	public override void Exit()
	{

		// 1. Shut down pathfinding completely
		if (controller.aiLerp != null)
		{
			controller.aiLerp.canSearch = false;
			controller.aiLerp.destination = controller.transform.position; // Force anchor to current spot
		}

		// 2. Kill velocity on EntityMover to be safe
		controller.EntityMover?.SetMoveDirection(Vector2.zero);

		// 3. Force animator to idle
		controller.EntityAnimator?.SetMoveAnimation(Vector2.zero);

		// 4. Log the state history
		stateMachine.SetPreviousState(this);
	}
}
