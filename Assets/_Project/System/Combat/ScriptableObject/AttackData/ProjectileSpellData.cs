using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "SpellData/ProjectileSpellData")]
public class ProjectileSpellData : SpellData
{
    [Header("Projectile")] 
    public float projectileSpeed = 5f;
    public float projectileLifetime = 5f;
    public float projectileHeight = 0f;
}
