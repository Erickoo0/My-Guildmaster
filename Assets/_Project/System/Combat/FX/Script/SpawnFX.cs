using UnityEngine;

public class SpawnFX : MonoBehaviour
{
    private float _despawnTimeMax;
    private float _despawnTime;
    
    private void Update()
    {
        if (_despawnTime <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            _despawnTime -= Time.deltaTime;
        }
    }

    public void SetupSpawnFX(float lifetime)
    {
        _despawnTimeMax = lifetime;
        _despawnTime = _despawnTimeMax;
    }
}
