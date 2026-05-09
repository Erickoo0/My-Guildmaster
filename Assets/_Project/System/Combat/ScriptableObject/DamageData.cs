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
    public int maxEnemiesHitCount;
    
    // Constructor to easily create damage types on the fly
    public DamageData(float amount, Vector2 direction, float force, float duration, float height, DamageType dmgDamageType, GameObject from = null, int hitCount = 1)
    {
        damageAmount = amount;
        hitDirection = direction;
        knockbackForce = force;
        knockbackDuration = duration;
        knockbackHeight = height;
        damageType = dmgDamageType;
        source = from;
        maxEnemiesHitCount = hitCount;
    }
}
