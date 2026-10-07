using System;
using UnityEngine;
/// <summary>
/// API for handling Entity Animations and broadcasting Animation events for other systems to listen to
/// </summary>
[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour, IFaceable
{

	// Cache animator strings as interger hashes for performance
	private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
	private static readonly int IsSittingHash = Animator.StringToHash("IsSitting");
	private static readonly int InputXHash = Animator.StringToHash("InputX");
	private static readonly int InputYHash = Animator.StringToHash("InputY");
	private static readonly int LastInputXHash = Animator.StringToHash("LastInputX");
	private static readonly int LastInputYHash = Animator.StringToHash("LastInputY");

	private static readonly int IdleMultiHash = Animator.StringToHash("IdleMulti");
	private static readonly int RunMultiHash = Animator.StringToHash("RunMulti");
	private static readonly int AttackMultiHash = Animator.StringToHash("AttackMulti");
	[HideInInspector] public Animator animator;

	[Header("Animation Speed Settings")]
	[SerializeField] private float IdleSpeedMultiplier = 1f;
	[SerializeField] private float RunSpeedMultiplier = 1f;
	[SerializeField] private float AttackSpeedMultiplier = 1f;

	private readonly float _moveThreshold = 0.25f;
	private int _currentActionBoolHash;
	private bool isEventRequested = false;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		ApplySpeedMultipliers();
	}

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

	public void ApplySpeedMultipliers()
	{
		animator.SetFloat(IdleMultiHash, IdleSpeedMultiplier);
		animator.SetFloat(RunMultiHash, RunSpeedMultiplier);
		animator.SetFloat(AttackMultiHash, AttackSpeedMultiplier);
	}

	public void SetSpeedMultipliers(float idle, float run, float attack)
	{
		IdleSpeedMultiplier = idle;
		RunSpeedMultiplier = run;
		AttackSpeedMultiplier = attack;
		ApplySpeedMultipliers();
	}

	public void SetMoveAnimation(Vector2 moveDirection)
	{
		// Safety Check
		if (animator == null) return;

		bool isRunning = moveDirection.sqrMagnitude > (_moveThreshold*_moveThreshold);
		animator.SetBool(IsRunningHash, isRunning);

		if (isRunning)
		{
			animator.SetFloat(InputXHash, moveDirection.x);
			animator.SetFloat(InputYHash, moveDirection.y);
			animator.SetFloat(LastInputXHash, moveDirection.x);
			animator.SetFloat(LastInputYHash, moveDirection.y);
		}
	}

	// Ver 1: Executes the face direction change
	public void FaceDirection(Vector2 lookDirection)
	{
		// Safety Check
		if (animator == null || lookDirection == Vector2.zero) return;

		// Forcing a direction usually means we are stationary
		animator.SetBool(IsRunningHash, false);

		// SNAP TO 4-WAY CARDINAL DIRECTION
		Vector2 snappedDirection = SnapToCardinal(lookDirection);

		animator.SetFloat(InputXHash, snappedDirection.x);
		animator.SetFloat(InputYHash, snappedDirection.y);
		animator.SetFloat(LastInputXHash, snappedDirection.x);
		animator.SetFloat(LastInputYHash, snappedDirection.y);
	}

	private Vector2 SnapToCardinal(Vector2 rawDirection)
	{
		// Strict > ensures we favor horizontal if exactly diagonal
		return Mathf.Abs(rawDirection.x) > Mathf.Abs(rawDirection.y)
			? new Vector2(Mathf.Sign(rawDirection.x), 0)
			: new Vector2(0, Mathf.Sign(rawDirection.y));
	}

	public void SetSpellAnimation(int boolHash)
	{
		_currentActionBoolHash = boolHash;
		isEventRequested = false;
		animator.SetBool(_currentActionBoolHash, true);
	}

	public void SetSitAnimation(bool isSitting)
	{
		if (animator == null)
			return;

		if (isSitting)
			animator.SetBool(IsRunningHash, false);
		animator.SetBool(IsSittingHash, isSitting);

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
