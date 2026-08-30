using System;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour, IFaceable
{
	[HideInInspector] public Animator animator;

	private readonly float _moveThreshold = 0.25f;
	private int _currentActionBoolHash;

	private bool isEventRequested = false;

	private void Start() => animator = GetComponent<Animator>();

	// Ver 2: Converts a FacingDirection ENUM to a raw Vector2
	public void FaceDirection(FacingDirection lookDirection)
	{
		if (lookDirection == FacingDirection.None) return;
		// Convert Enum to a Vector2 
		FaceDirection(lookDirection.ToVector2());
	}
	public event Action OnAnimationEventRequested;
	public event Action OnAnimationFinished;
	public event Action OnAnimationCanceled;

	public void SetMoveAnimation(Vector2 moveDirection)
	{
		// Safety Check
		if (animator == null) return;

		bool isRunning = moveDirection.sqrMagnitude > (_moveThreshold*_moveThreshold);
		animator.SetBool("IsRunning", isRunning);

		if (isRunning)
		{
			animator.SetFloat("InputX", moveDirection.x);
			animator.SetFloat("InputY", moveDirection.y);
			animator.SetFloat("LastInputX", moveDirection.x);
			animator.SetFloat("LastInputY", moveDirection.y);
		}
	}

	// Ver 1: Executes the face direction change
	public void FaceDirection(Vector2 lookDirection)
	{
		// Safety Check
		if (animator == null || lookDirection == Vector2.zero) return;

		// Forcing a direction usually means we are stationary
		animator.SetBool("IsRunning", false);

		// SNAP TO 4-WAY CARDINAL DIRECTION
		Vector2 snappedDirection = SnapToCardinal(lookDirection);

		animator.SetFloat("InputX", snappedDirection.x);
		animator.SetFloat("InputY", snappedDirection.y);
		animator.SetFloat("LastInputX", snappedDirection.x);
		animator.SetFloat("LastInputY", snappedDirection.y);
	}

	private Vector2 SnapToCardinal(Vector2 rawDirection)
	{
		// Strict > ensures we favor horizontal if exactly diagonal
		return Mathf.Abs(rawDirection.x) > Mathf.Abs(rawDirection.y)
			? new Vector2(Mathf.Sign(rawDirection.x), 0)
			: new Vector2(0, Mathf.Sign(rawDirection.y));
	}

	public void StartSpellAnimation(int boolHash)
	{
		_currentActionBoolHash = boolHash;
		isEventRequested = false;
		animator.SetBool(_currentActionBoolHash, true);
	}

	public void OnAttackAnimationFinished()
	{
		if (_currentActionBoolHash != 0)
		{
			animator.SetBool(_currentActionBoolHash, false);
		}

		isEventRequested = false;
		OnAnimationFinished?.Invoke();
	}

	public void RequestAnimationEvent()
	{
		if (isEventRequested) return;
		isEventRequested = true;
		OnAnimationEventRequested?.Invoke();
	}

	public void RequestAnimationCancel()
	{
		isEventRequested = false;
		OnAnimationCanceled?.Invoke();
	}
}
