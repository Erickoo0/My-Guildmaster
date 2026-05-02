using UnityEngine;

[RequireComponent(typeof(Animator), (typeof(EntityMover)))]
public class EntityAnimator : MonoBehaviour
{
    public Animator animator;
    private EntityMover _entityMover;

    [Header("Animation Settings")] 
    [Tooltip("It true, snaps animations to 4 way cardinal directions")]
    [SerializeField] private bool snapToCardinalDirections;
    
    private readonly float _moveThreshold = 0.25f;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        _entityMover = GetComponent<EntityMover>();
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

    /// <summary>
    /// Forces the entity to look in a specific direction without input / walking
    /// Useful for dialogue interactions, cutscenes, etc
    /// </summary>
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
}
