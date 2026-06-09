using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNPCWanderState : State<NPCController>
{
    [Header("Location Settings")] 
    protected List<PointOfInterest> _poiList = new List<PointOfInterest>();
    protected PointOfInterest _selectedPOI;
    protected bool _IsMovingToEntrance = false;

    [Header("Pathing Logic")]
    [SerializeField] protected bool walkInSequence = false;
    protected int _currentPOIIndex = 0;
    
    [Header("Anti-Stuck Variables")]
    protected float _stuckTimer;
    protected readonly float _stuckTimerMax = 5f;
    protected Vector2 _lastPosition;
    protected readonly float _stuckThreshold = 0.1f;
    protected float _positionCheckTimer;
    protected readonly float _positionCheckInterval = 0.5f;
    
    // 1. Child classes must provide the correct POI IDs
    protected virtual List<string> GetPOITargetIDs() => new List<string>();
    
    public override void Enter()
    {
        // 1. Reset state flags
        _stuckTimer = 0f;
        _positionCheckTimer = 0f;
        
        // 2. Ask the POI Registry for the POI objects
        _poiList = POIRegistry.GetPOIByIDs(GetPOITargetIDs());
        
        // 3. Set Destination
        if (_poiList.Count > 0)
            SetNewDestination();
        
        // 4. Tell the AI to start calculating paths again
        controller.aiPath.canSearch = true;
        _lastPosition = controller.transform.position;
    }
    
    public override void Update()
    {
        // 1. Check if knocked back
        if (controller.EntityMover != null && controller.EntityMover.IsKnockedBack) return;

        // 2. Destination Arrival logic
        if (!controller.aiPath.pathPending && controller.aiPath.reachedEndOfPath)
        {
            OnReachedDestination();
            return;
        }
        
        // 3. Tell AIPath to calculate the next step
        controller.aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out _);
        
        // 4. Calculate Direction based on the NEXT position calculated by A*
        Vector2 moveDirection = ((Vector2)nextPos - (Vector2)controller.transform.position).normalized;
        
        // 4. Handle Overshooting: Check distance to the destination
        float distanceToTarget = Vector2.Distance(controller.transform.position, controller.aiPath.destination);
        if (distanceToTarget < controller.aiPath.endReachedDistance)
        {
            controller.EntityMover.SetMoveDirection(Vector2.zero);
        }
        else
        {
            controller.EntityMover.SetMoveDirection(moveDirection);
        }
        
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
        } 
        else // If not walk in sequence, pick a random destination
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
                _IsMovingToEntrance = true;
                _selectedPOI = transitNode;
            }
            else
            {
                Debug.LogError($"[{controller.gameObject.name}] is stuck! No route from {controller.currentLocation} to {ultimateDestination.Location}");
                return; 
            }
        } 
        else
        {
            // We are in the correct room. Walk straight to the destination.
            _IsMovingToEntrance = false;
            _selectedPOI = ultimateDestination;
        }
        
        // 4. Send the data to the A* Pathfinding
        controller.aiPath.destination = _selectedPOI.transform.position;
        controller.aiPath.SearchPath();
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
            }
            else
            {
                _stuckTimer = 0;
            }
            
            if (_stuckTimer > _stuckTimerMax)
            {
                Debug.Log("Entity stuck! Resetting path.");
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
                controller.aiPath.Teleport(teleportTarget.transform.position); // Teleport to the teleportTarget POI
                controller.currentLocation = teleportTarget.Location;
                controller.EntityAnimator.FaceDirection(teleportTarget.lookDirection);
            }
            else
                Debug.LogWarning($"[{controller.gameObject.name}] Teleport failed! Could not find POI with ID: '{_selectedPOI.TeleportPOI}' in the POIRegistry.");
        } 
        else
        {
            controller.currentLocation = _selectedPOI.Location;
            controller.EntityAnimator.FaceDirection(_selectedPOI.lookDirection);
        }
        
        // Check if we just arrived at the entrance
        if (_IsMovingToEntrance)
        {
            _IsMovingToEntrance = false;
            
            // Cast the state and trigger the skip idle time
            if (controller.IdleState is NPCIdleState idleState)
                idleState.SkipNextIdle();
        }
        
        stateMachine.ChangeState(controller.IdleState);
    }
    
    public override void Exit()
    {
        
        // 6. Shut down pathfinding and halt the EntityMover
        controller.aiPath.canSearch = false;
        
        if (controller.EntityMover != null)
            controller.EntityMover.SetMoveDirection(Vector2.zero);

        stateMachine.SetPreviousState(this);
    }
}
