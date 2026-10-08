using UnityEngine;
/// <summary>
/// A state representing the player sitting.
/// Locks movement, snaps position, and assigns the player to a seat
/// </summary>
public class PlayerSitState : State<ControllerPlayer>
{
	private int _originalSortingOrder;
	private InteractableSeat _seat;
	private Vector2 _seatPosition;
	public void Setup(InteractableSeat seat, Vector2 seatPosition)
	{
		_seat = seat;
		_seatPosition = seatPosition;
	}

	public override void Enter()
	{
		// 1. snap the players position to the seat position
		controller.transform.position = _seatPosition;

		// 2. Lock movement
		controller.EntityMover.SetMoveDirection(Vector2.zero);
		controller.EntityAnimator.SetMoveAnimation(Vector2.zero);
		controller.SetCanMove(false);

		// 3. adjust sorting orrder
		_originalSortingOrder = controller.PlayerSpriteRenderer.sortingOrder;
		controller.PlayerSpriteRenderer.sortingOrder += _seat.SortingOrderOffset;

		// 4. Disable collision
		if (controller.PlayerCollider != null && _seat.SeatColliders != null)
			foreach (Collider2D seatCollider in _seat.SeatColliders)
				Physics2D.IgnoreCollision(controller.PlayerCollider, seatCollider, true);

		// 5. Set the sitting animation
		controller.EntityAnimator.SetSitAnimation(true);
	}

	public override void Exit()
	{
		// 1. Restore movement
		controller.SetCanMove(true);

		// 2. Restore collision
		if (controller.PlayerCollider != null && _seat.SeatColliders != null)
			foreach (Collider2D seatCollider in _seat.SeatColliders)
				Physics2D.IgnoreCollision(controller.PlayerCollider, seatCollider, false);

		// 3. Unset the sitting animation
		controller.PlayerSpriteRenderer.sortingOrder = _originalSortingOrder;
		controller.EntityAnimator.SetSitAnimation(false);

		// 4. Free the seat
		if (_seat != null)
		{
			_seat.FreeSeat();
			_seat = null;
		}
	}
}
