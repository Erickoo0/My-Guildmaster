using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Water,
    Earth,
    Lightning,
    Holy,
    Shadow
}

[System.Serializable]
public struct DamageData
{
    public float damageAmount;
    public Vector2 hitDirection;
    public float knockbackForce;
    public float knockbackDuration;
    public float knockbackHeight;
    public DamageType damageType;
    public GameObject source;
    
    // Constructor to easily create damage types on the fly
    public DamageData(float amount, Vector2 direction, float force, float duration, float height, DamageType dmgDamageType, GameObject from = null)
    {
        damageAmount = amount;
        hitDirection = direction;
        knockbackForce = force;
        knockbackDuration = duration;
        knockbackHeight = height;
        damageType = dmgDamageType;
        source = from;
    }
}

public struct CombatContext
{
    public GameObject source;
    public Vector2 mousePosition;
    public Vector2 userPosition;
    public Vector2 facingDirection;
}
