using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCWorkState : BaseNPCWorkState
{
    protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.WorkPOIList;

    public override void Enter()
    {
        // 1. Reset state flags
        _stuckTimer = 0f;
        _positionCheckTimer = 0f;
        // If the NPC did not come from IdleState, reset arrived flag
        if (stateMachine.PreviousState != controller.IdleState)
            _arrivedMainDestination = false;
        
        // 2. Ask the POI Registry for the POI objects
        _poiList = POIRegistry.GetPOIByIDs(GetPOITargetIDs());
        
        // 3. Set Destination
        if (_poiList.Count > 0)
            SetNewDestination();
        
        // 4. Tell the AI to start calculating paths again
        controller.aiPath.canSearch = true;
        _lastPosition = controller.transform.position;
    }
    
    protected override void SetNewDestination()
    {
        if (_poiList == null || _poiList.Count == 0) return;
        
        if (!_arrivedMainDestination)
        {
            // Move to the first POI in the list (Entrance)
            _selectedPOI = _poiList[0];
        }
        else if (_arrivedMainDestination && _poiList.Count >= 2)
        {
            // Select a random POI from the list (other than entrance and exit)
            _selectedPOI = _poiList[Random.Range(2, _poiList.Count)];
        }
        
        controller.aiPath.destination = _selectedPOI.transform.position;
        controller.aiPath.SearchPath();
    }

    protected override void OnReachedDestination()
    {
        if (!_arrivedMainDestination)
        {
            // If arrived at work entrance, teleport to the second POI (Exit POI)
            // then set new destination
            _arrivedMainDestination = true;
            Vector2 teleportPosition = _poiList[1].transform.position;
            controller.aiPath.Teleport(teleportPosition);
            SetNewDestination();
            return;
        }
        else if (_arrivedMainDestination)
        {
            stateMachine.ChangeState(controller.IdleState);
        }
    }
}
