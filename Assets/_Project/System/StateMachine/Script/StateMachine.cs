using UnityEngine;
public class StateMachine : MonoBehaviour
{
	public State CurrentState { get; private set; }
	public State PreviousState { get; private set; }

	// Starts the initial state and calls it
	public void SetupState(State startingState)
	{
		if (startingState == null) return;

		CurrentState = startingState;
		CurrentState.Enter();
	}

	public void ChangeState(State newState)
	{
		if (newState == null) return;

		CurrentState.Exit();
		CurrentState = newState;
		CurrentState.Enter();
	}

	public void UpdateState()
	{
		if (CurrentState == null) return;

		CurrentState.HandleInput();
		CurrentState.Update();
	}

	public void FixedUpdateState()
	{
		if (CurrentState == null) return;

		CurrentState.PhysicsUpdate();
	}

	public void SetPreviousState(State previousState)
	{
		PreviousState = previousState;
	}
}
