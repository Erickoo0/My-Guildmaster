using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 _projectileDirection;
    private float _projectileSpeed;
    private DamageData _damageData;
    private bool _hasHit;

    public void Setup(Vector2 projectDirection, float projectileSpeed, float projectileLifetime, DamageData damageData)
    {
        _projectileDirection = projectDirection;
        _projectileSpeed = projectileSpeed;
        _damageData = damageData;
        
        if (TryGetComponent(out HitBox hitBox))
            hitBox.Setup(_damageData);

        // Rotate arrow to face direction
        float angle = Mathf.Atan2(projectDirection.y, projectDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, projectileLifetime);
    }

    private void Update()
    {
        transform.Translate(_projectileDirection * _projectileSpeed * Time.deltaTime, Space.World);
    }
}