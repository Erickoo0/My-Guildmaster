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
    [Header("Base Data")]
    public float damageAmount;
    public Vector2 hitDirection;
    public Vector2 hitImpactPoint;
    public float knockbackForce;
    public float knockbackDuration;
    public float knockbackHeight;
    public DamageType damageType;
    public GameObject source;
    
    [Header("Behavior Data")]
    public bool hitOncePerTarget;
    public bool destroyOnMaxHits;
    public int maxEnemiesHitCount;
    
    // Constructor to easily create damage types on the fly
    public DamageData(float amount, Vector2 direction, Vector2 impactPoint, float force, float duration, float height, 
        DamageType dmgType, GameObject from, int hitCount, bool oncePerTarget, bool destroyOnMax)
    {
        damageAmount = amount;
        hitDirection = direction;
        hitImpactPoint = impactPoint;
        knockbackForce = force;
        knockbackDuration = duration;
        knockbackHeight = height;
        damageType = dmgType;
        source = from;
        maxEnemiesHitCount = hitCount;
        hitOncePerTarget = oncePerTarget;
        destroyOnMaxHits = destroyOnMax;
    }
}
