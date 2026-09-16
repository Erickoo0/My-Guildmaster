using System;

public enum StateType { Idle, Wander, Chase, Action }

[Serializable]
public abstract class State
{
	protected StateMachine stateMachine; // Reference to the state machine

	// Contracts that each state must fulfill
	public virtual void Enter() {}
	public virtual void Update() {}
	public virtual void PhysicsUpdate() {}
	public virtual void HandleInput() {}
	public virtual void Exit() {}
}

[Serializable]
public abstract class State<T> : State where T : ControllerBase
{
	// Specialized Reference holds the controller (ControllerPlayer / ControllerEntity)
	protected T controller;


	public virtual void Setup(T controller, StateMachine stateMachine)
	{
		this.controller = controller;
		this.stateMachine = stateMachine;
	}
}
