using UnityEngine;

public class HitBoxAOE : HitBox
{
    protected override void CalculateImpactPhysics(Collider2D other, out Vector2 knockbackDirection, out Vector2 impactPoint)
    {
        // Knockback pushes away from the center of the explosion
        knockbackDirection = (other.transform.position - transform.position).normalized;
        
        // Impact point is where the explosion touches the enemy's collider
        impactPoint = other.ClosestPoint(transform.position);
    }
}
