using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;


public class FloatingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FloatingText textPrefab;
    
    [Header("Pool Settings")]
    [SerializeField] private int defaultPoolSize; // Bigger Pool = More Memory
    [SerializeField] private int maxSize = 50;
    
    [Header("Position Settings")]
    [SerializeField] private float yOffset = 1.5f;
    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float randomYRange = 0.2f;
    [SerializeField] private float randomXRange = 0.5f;


    // Custom Object Pool using a Queue
    private IObjectPool<FloatingText> _textPool;


    private void Awake()
    {
        // Initialize the object pool
        _textPool = new ObjectPool<FloatingText>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxSize
            );
    }
    
    // Subscribes to CombatEvents
    private void OnEnable() => EventBus.OnFloatingTextRequested += SpawnFloatingNumber;
    private void OnDisable() => EventBus.OnFloatingTextRequested -= SpawnFloatingNumber;


    private void SpawnFloatingNumber(int amount, Vector3 position)
    {
        // 1. Get an object from the pool
        FloatingText textObject = _textPool.Get();
        
        // 2.Apply offsets
        position.y += yOffset + Random.Range(-randomYRange, randomYRange);
        position.x += xOffset + Random.Range(-randomXRange, randomXRange);
        
        // 3. Initialize it, passing the pool so the object knows where to return
        textObject.Initialize(amount, position, _textPool);
    }
    
    //----POOL CALLBACK METHODS----

    private FloatingText CreatePooledItem() => Instantiate(textPrefab, transform);
    private void OnTakeFromPool(FloatingText textObject) => textObject.gameObject.SetActive(true);
    private void OnReturnedToPool(FloatingText textObject) => textObject.gameObject.SetActive(false);
    private void OnDestroyPoolObject(FloatingText textObject) => Destroy(textObject.gameObject);
}

