using UnityEngine;

[System.Serializable]
public class NPCIdleState : BaseNPCIdleState
{
    private NPCScheduleController _scheduleController;
    
    private float _idleTime;
    private float minIdleTime = 10f;
    private float maxIdleTime = 20f;
    private bool _skipNextIdle = false;
    public void SkipNextIdle() => _skipNextIdle = true;
    
    public override void Enter()
    {
        if (_scheduleController == null) _scheduleController = controller.GetComponent<NPCScheduleController>();
        
        controller.EntityMover.SetMoveDirection(Vector2.zero);

        if (_skipNextIdle)
        {
            _idleTime = 0f;
            _skipNextIdle = false;
        }
        else
            _idleTime = Random.Range(minIdleTime,maxIdleTime);
    }

    public override void Update()
    {
        if (_idleTime > 0) _idleTime -= Time.deltaTime;
        else
            stateMachine.ChangeState(_scheduleController.CurrentScheduledState);
        
    }
    
    public override void PhysicsUpdate() { }
    
    public override void HandleInput() { }

    public override void Exit()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        
        stateMachine.SetPreviousState(this);
    }
}
