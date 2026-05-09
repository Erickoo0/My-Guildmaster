using UnityEngine;

public class HitBoxContact : HitBox
{
    protected override Vector2 GetKnockbackDirection(Collider2D other)
    {
        // 1. Where is the core of our attack?
        Vector2 entityCenter = entityCollider.bounds.center;
        
        // 2. Find the exact point on the enemy's collider edge closest to our attack
        Vector2 attackImpactPoint = other.ClosestPoint(entityCenter);
        
        // 3. Save this point to our damage data so the HurtBox can use it for particles!
        damageData.hitImpactPoint = attackImpactPoint;
        
        // 4. Direction calculation
        Vector2 victimCenter = other.bounds.center;
        Vector2 direction = victimCenter - attackImpactPoint;
        
        
        return direction == Vector2.zero ? Vector2.up : direction.normalized;
    }
}