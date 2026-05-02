using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "AttackData/ProjectileAttackData")]
public class ProjectileAttackData : AttackData
{
    [Header("Projectile")] 
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float projectileLifetime = 5f;
}
