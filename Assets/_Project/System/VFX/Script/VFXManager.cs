using UnityEngine;
using UnityEngine.Pool;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VFX defaultDeathVFX;

    [Header("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 15;
    [SerializeField] private int maxSize = 50;
    
    // Global DeathVFX Particle System Pool
    private IObjectPool<VFX> _deathVFXPool;

    private void Awake()
    {
        _deathVFXPool = new ObjectPool<VFX>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false, // Set to false in production to save CPU
            defaultCapacity: defaultPoolSize,
            maxSize: maxSize
            );
    }

    private void OnEnable() => EventBus.OnEntityDeathRequested += HandleEntityDeath;

    private void OnDisable() => EventBus.OnEntityDeathRequested -= HandleEntityDeath;

    private void HandleEntityDeath(GameObject entity)
    {
        // Safety Check
        if (entity == null) return;
        
        // 1. Get an object from the pool
        VFX deathVFX = _deathVFXPool.Get();
        
        // 2. Move it to the entity's position
        deathVFX.transform.position = entity.transform.position;
    }
    
    //----POOL CALLBACK METHODS----
    
    private VFX CreatePooledItem()
    {
        VFX newPooledItem = Instantiate(defaultDeathVFX, transform);
        newPooledItem.SetPool(_deathVFXPool);
        return newPooledItem;
    }

    private void OnTakeFromPool(VFX pooledItem) => pooledItem.gameObject.SetActive(true);
    private void OnReturnedToPool(VFX pooledItem) => pooledItem.gameObject.SetActive(false);
    private void OnDestroyPoolObject(VFX pooledItem) => Destroy(pooledItem.gameObject);
}
