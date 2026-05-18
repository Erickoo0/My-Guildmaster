using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNPCWanderState : State<NPCController>
{
    [Header("Location Settings")] 
    protected List<PointOfInterest> _poiList = new List<PointOfInterest>();
    protected PointOfInterest _selectedPOI;
    protected bool _arrivedMainDestination = false;
    
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
        _arrivedMainDestination = false;
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
        controller.aiPath.MovementUpdate(Time.deltaTime, out Vector3 nextPos, out Quaternion nextRot);
        
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
        if (_poiList != null && _poiList.Count > 0)
            _selectedPOI = _poiList[Random.Range(0, _poiList.Count)];
        
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
        // Face Direction logic
        controller.EntityAnimator.FaceDirection((_selectedPOI.lookDirection));
        
        _arrivedMainDestination = true;
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
