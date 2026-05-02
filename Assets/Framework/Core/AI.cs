using UnityEngine;
using Pathfinding;

public class EntityAI : MonoBehaviour
{
    private AIPath _path;
    private AIDestinationSetter _setter;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform target;

    private void Awake()
    {
        _path = GetComponent<AIPath>();
        _setter = GetComponent<AIDestinationSetter>();
        
        // Initialize AIPath settings for snappy RPG movement
        _path.maxSpeed = moveSpeed;
        _path.maxAcceleration = 100f; // High acceleration prevents sliding into walls
        _path.pickNextWaypointDist = 0.5f; // Lower values = tighter cornering
        _path.slowdownDistance = 0.5f;
    }

    private void Start()
    {
        // Tell the setter who to follow
        if (target != null)
        {
            _setter.target = target;
        }
    }

    // You no longer need Update() to set the destination!
    // The AIDestinationSetter handles it internally.
}