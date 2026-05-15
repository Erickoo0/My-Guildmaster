using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class HitBoxContact : HitBox
{
    protected override void CalculateImpactPhysics(Collider2D other, out Vector2 knockbackDirection, out Vector2 impactPoint)
    {
        // 1. Where is the core of our attack?
        Vector2 entityCenter = entityCollider.bounds.center;
        
        // 2. Find the exact point on the enemy's collider edge closest to our attack
        impactPoint = other.ClosestPoint(entityCenter);
        
        // 3. Direction calculation
        Vector2 victimCenter = other.bounds.center;
        Vector2 rawDirection = victimCenter - impactPoint;
        
        knockbackDirection = rawDirection == Vector2.zero ? Vector2.up : rawDirection.normalized;
    }
}