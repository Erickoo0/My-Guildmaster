using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour, IFaceable
{
    [HideInInspector] public Animator animator;
    private EntityMover _entityMover;

    [Header("Animation Settings")] 
    [Tooltip("It true, snaps animations to 4 way cardinal directions")]
    [SerializeField] private bool snapToCardinalDirections;
    private bool isEventRequested = false;
    
    private readonly float _moveThreshold = 0.25f;
    
    private int _currentActionBoolHash;
    
    public event System.Action OnAnimationEventRequested;
    public event System.Action OnAnimationFinished;
    public event System.Action OnAnimationCanceled;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        _entityMover = GetComponentInParent<EntityMover>();
    }

    private void Update()
    {
        // Read the direction from the EntityMover
        Vector2 moveDirection = _entityMover.MoveDirection;
        
        // Only set walking if move is significant
        bool IsRunning = moveDirection.magnitude > _moveThreshold; 
        animator.SetBool("IsRunning", IsRunning);
        
        if (IsRunning)
        {
            Vector2 animationDirection = snapToCardinalDirections ? GetSnappedDirection(moveDirection) : moveDirection;
            
            animator.SetFloat("InputX", animationDirection.x);
            animator.SetFloat("InputY", animationDirection.y);
            
            // Store the last facing direction for idle animations
            animator.SetFloat("LastInputX", animationDirection.x);
            animator.SetFloat("LastInputY", animationDirection.y);
        }
    }
    
    // Ver 1: Executes the face direction change
    public void FaceDirection(Vector2 lookDirection)
    {
        // Safety check
        if (animator == null || lookDirection == Vector2.zero) return;
        
        Vector2 animDir = snapToCardinalDirections ? GetSnappedDirection(lookDirection) : lookDirection;

        animator.SetFloat("InputX", animDir.x);
        animator.SetFloat("InputY", animDir.y);
        animator.SetFloat("LastInputX", animDir.x);
        animator.SetFloat("LastInputY", animDir.y);
    }

    // Ver 2: Converts a FacingDirection ENUM to a raw Vector2
    public void FaceDirection(FacingDirection lookDirection)
    {
        if (lookDirection == FacingDirection.None) return;
        // Convert Enum to a Vector2 
        FaceDirection(lookDirection.ToVector2());
    }

    private Vector2 GetSnappedDirection(Vector2 moveInput)
    {
        // Favors horizontal animation if diagonal input is perfectly equal
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            return new Vector2(Mathf.Sign(moveInput.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(moveInput.y));
        }
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