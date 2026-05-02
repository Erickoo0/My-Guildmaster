using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 _direction;
    private float _speed;
    private DamageData _damageData;
    private bool _hasHit;

    public void Setup(Vector2 dir, float speed, float lifetime, DamageData data)
    {
        _direction = dir;
        _speed = speed;
        _damageData = data;

        // Rotate arrow to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasHit) return;

        // Assuming your HitBox logic or Health system is here
        ITargetable target = collision.GetComponentInParent<ITargetable>();
        if (target != null && collision.gameObject != _damageData.source)
        {
            _hasHit = true;
            // Here you would call your damage logic, e.g.:
            // target.TakeDamage(_damageData);
            
            Debug.Log($"Arrow hit {collision.gameObject.name}");
            Destroy(gameObject); 
        }
    }
}