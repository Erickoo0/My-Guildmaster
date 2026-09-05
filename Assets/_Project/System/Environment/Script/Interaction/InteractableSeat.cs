using UnityEngine;
/// <summary>
/// An interactable component that defines a sitable prop
/// </summary>
public class InteractableSeat : MonoBehaviour, IInteractable
{
	[SerializeField] private bool _isInteractable = true;
	[SerializeField] private Transform _seatPosition;

	public int SortingOrderOffset = 1;

	public Collider2D[] SeatColliders;
	private ControllerBase _sittingEntity; // Track who is currently sitting

	private void Awake() => SeatColliders = GetComponents<Collider2D>();

	public bool CanInteract() => _isInteractable;

	public void Interact(ControllerPlayer player)
	{
		if (!_isInteractable) return;

		if (_sittingEntity == null)
		{
			State currentState = player.StateMachine.CurrentState;

			// 1. Check if player is in a valid state
			if (currentState != player.IdleState && currentState != player.MoveState)
				return;

			// 2. Cast the state to PlayerSitState
			if (player.SitState is PlayerSitState sitState)
			{
				// 3. Setup the data
				_sittingEntity = player;
				sitState.Setup(this, GetInteractionPoint());

				// 4. Transition to state
				player.StateMachine.ChangeState(sitState);
			}
		}

		// 5. If already sitting
		else if (_sittingEntity == player)
			player.StateMachine.ChangeState(player.IdleState);
	}

	public void FreeSeat() => _sittingEntity = null;

	public void SetInteractable(bool value) => _isInteractable = value;

	private Vector3 GetInteractionPoint() => _seatPosition != null ? _seatPosition.position : transform.position;
}
