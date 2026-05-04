using UnityEngine;

public class HitBoxContact : HitBox
{
    protected override Vector2 GetKnockbackDirection(Collider2D other)
    {
        Vector2 direction = (Vector2)other.transform.position - (Vector2)transform.position;
        
        return direction == Vector2.zero ? Vector2.up : direction.normalized;
    }
}