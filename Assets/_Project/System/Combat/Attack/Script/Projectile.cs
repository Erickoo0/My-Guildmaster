using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Transform _projectileVisual;
    private enum MovementType {Linear, Curved}
    private MovementType _movementType;
    private bool _destroyOnCollisions;
    private Vector3 _linearDirection;
    
    private Vector3 _projectileStartPosition;
    private Vector3 _projectileTargetPosition;
    
    private float _projectileSpeed;
    private float _projectileMaxRelativeHeight;

    private DamageData _damageData;
    private HitBox _hitBox;
    
    private AnimationCurve _projectileCurve;

    // Track movement progress
    private float _totalDuration;
    private float _currentDuration;

    public void Setup(Vector3 projectileTargetPosition, float projectileSpeed, float projectileLifetime, AnimationCurve projectileCurve, float projectileMaxHeight, bool projectileDestroy, DamageData damageData)
    {
        // 1. Pass the data
        _projectileStartPosition = transform.position;
        _projectileTargetPosition = projectileTargetPosition;
        _projectileSpeed = projectileSpeed;
        _damageData = damageData;
        _destroyOnCollisions = projectileDestroy;
        
        // 2. Get the Hitbox
        if (TryGetComponent(out _hitBox))
            _hitBox.Setup(_damageData);
        
        // 3. Pass the lifetime
        Destroy(gameObject, projectileLifetime);
        
        // 4. Calculate flat 2d travel direction
        _linearDirection = (_projectileTargetPosition - _projectileStartPosition).normalized;
        if (_linearDirection== Vector3.zero) _linearDirection = Vector3.right;
        FaceTargetDirection(_linearDirection);
        
        // 4. Set the projectile type
        if (projectileMaxHeight <= 0f)
        {
            _movementType = MovementType.Linear;
            
            FaceTargetDirection(_linearDirection);
            
            // Enable hitbox immediately
            if (_hitBox != null) _hitBox.enableHitbox = true;
        } 
        else if (projectileMaxHeight > 0f)
        {
            _movementType = MovementType.Curved;

            _projectileTargetPosition = projectileTargetPosition;
            _projectileCurve = projectileCurve;
            _currentDuration = 0f;
            
            // Disable hitbox during movement
            if (_hitBox != null) _hitBox.enableHitbox = false;
            
            // calculate duration
            float distance = Vector3.Distance(_projectileStartPosition, _projectileTargetPosition);
            _totalDuration = distance > 0 ? (distance / projectileSpeed) : 0f;
            
            // Calculate height
            _projectileMaxRelativeHeight = distance * projectileMaxHeight;
        }
    }

    private void Update()
    {
        if (_movementType == MovementType.Linear)
        {
            UpdateLinearMovement();
        }
        else if (_movementType == MovementType.Curved)
        {
            UpdateCurvedMovement();
        }
    }

    private void UpdateLinearMovement()
    {
        transform.position += _linearDirection * (_projectileSpeed * Time.deltaTime);
    }

    private void UpdateCurvedMovement()
    {
        // 1. Accumulate elapsed time and normalize it between 0.0 and 1.0
        _currentDuration += Time.deltaTime;
        float t = Mathf.Clamp01(_currentDuration / _totalDuration);
        
        // 2. Move the ROOT game object (Hitbox & Shadow) linearly
        transform.position = Vector3.Lerp(_projectileStartPosition, _projectileTargetPosition, t);
        
        //3. Move the projectile visual vertically
        if (_projectileVisual != null)
        {
            float heightOffset = _projectileCurve.Evaluate(t) * _projectileMaxRelativeHeight;
            _projectileVisual.localPosition = new Vector3(0, heightOffset, 0);
        }
        
        // 4. Destination reached logic
        if (t >= 1f)
            OnTargetReached();
    }

    private void FaceTargetDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (_projectileVisual != null)
        {
            _projectileVisual.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else // Fallback
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTargetReached()
    {
        if (_movementType == MovementType.Curved)
        {
            // 1. Activate the hitbox
            if (_hitBox != null) _hitBox.enableHitbox = true;
            
            // 2. Hide the visual immediately
            if (_projectileVisual != null) _projectileVisual.gameObject.SetActive(false);
            
            // 3. Destroy after a tiny delay for hitbox effect to register
            Destroy(gameObject, 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_destroyOnCollisions) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Collisions"))
        {
            Destroy(gameObject);
        }
    }
}