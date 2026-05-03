using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    private EntityMover _entityMover;

    [Header("Animation Settings")] 
    [Tooltip("It true, snaps animations to 4 way cardinal directions")]
    [SerializeField] private bool snapToCardinalDirections;
    private bool isEventRequested = false;
    
    private readonly float _moveThreshold = 0.25f;
    
    public event System.Action OnAnimationEventRequested;
    
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
    
    public void FaceDirection(Vector2 lookDirection)
    {
        // Safety check
        if (animator == null || lookDirection == Vector2.zero) return;
        
        Vector2 animDir = snapToCardinalDirections ? GetSnappedDirection(lookDirection) : lookDirection;

        animator.SetFloat("LastInputX", animDir.x);
        animator.SetFloat("LastInputY", animDir.y);
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

    public void OnAttackAnimationFinished()
    {
        animator.SetBool("IsAttacking", false);
        isEventRequested = false;
    }

    public void RequestAnimationEvent()
    {
        if (isEventRequested) return;
        isEventRequested = true;
        OnAnimationEventRequested?.Invoke();
    }
}
