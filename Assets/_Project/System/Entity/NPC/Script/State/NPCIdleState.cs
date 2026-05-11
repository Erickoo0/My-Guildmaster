using UnityEngine;

[System.Serializable]
public class NPCIdleState : BaseNPCIdleState
{
    private NPCScheduleController _scheduleController;
    
    private float _idleTime;
    
    public override void Enter()
    {
        _scheduleController = controller.GetComponent<NPCScheduleController>();
        
        controller.EntityMover.SetMoveDirection(Vector2.zero); 
        _idleTime = Random.Range(0.5f, 1.5f);
    }

    public override void Update()
    {
        // if  no target, wait for a random time, then switch to wander state
        if (_idleTime > 0) _idleTime -= Time.deltaTime;
        else
        {
            Debug.Log($"Change State to {_scheduleController.CurrentScheduledState.GetType().Name}");
            stateMachine.ChangeState(_scheduleController.CurrentScheduledState);
        }
    }
    
    public override void PhysicsUpdate() { }
    
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        
        stateMachine.SetPreviousState(this);
    }
}
