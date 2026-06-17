using System;
using UnityEngine;
using System.Collections.Generic;

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
    private AnimationCurve _projectileCurve;
    private float _projectileScale;

    private float _totalDuration;
    private float _currentDuration;
    
    private HitBox _hitBox;
    private GameObject _user;

    public void Setup(Vector3 projectileTargetPosition, float projectileSpeed, float projectileLifetime, AnimationCurve projectileCurve, 
        float projectileMaxHeight, GameObject user, List<Effect> onHitEffects, int maxHits, bool hitOnce, bool destroyOnMax, float projectileScale)
    {
        // 1. Pass the cached data
        _projectileStartPosition = transform.position;
        _projectileTargetPosition = projectileTargetPosition;
        _projectileSpeed = projectileSpeed;
        _destroyOnCollisions = destroyOnMax;
        _projectileScale = projectileScale;
        _user = user;
        
        // 2. Get the Hitbox and hand it a reference to this projectile
        if (TryGetComponent(out _hitBox))
        {
            _hitBox.Setup(user, onHitEffects, maxHits, hitOnce, destroyOnMax); 
        }
        
        // 3. Pass the lifetime
        Destroy(gameObject, projectileLifetime);
        
        // 4. Calculate flat 2d travel direction
        _linearDirection = (_projectileTargetPosition - _projectileStartPosition).normalized;
        if (_linearDirection == Vector3.zero) _linearDirection = Vector3.right;
        FaceTargetDirection(_linearDirection);
        
        // 5. Set the projectile type
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
}