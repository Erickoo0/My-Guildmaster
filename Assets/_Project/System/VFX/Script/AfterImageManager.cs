using UnityEngine;
using UnityEngine.Pool;
/// <summary>
/// Handles the object pool and instaniation of AfterImages
/// </summary>
public class AfterImageManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private AfterImage afterImagePrefab;

	[Header("Pool Settings")]
	[SerializeField] private int defaultPoolSize = 10;
	[SerializeField] private int maxSize = 50;

	private IObjectPool<AfterImage> _afterImagePool;
	public static AfterImageManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		_afterImagePool = new ObjectPool<AfterImage>(
			createFunc: CreatePooledItem,
			actionOnGet: OnTakeFromPool,
			actionOnRelease: OnReturnedToPool,
			actionOnDestroy: OnDestroyPoolObject,
			collectionCheck: false, // Keep false to save CPU overhead
			defaultCapacity: defaultPoolSize,
			maxSize: maxSize
			);
	}

	public void SpawnAfterImage(Sprite sprite, Vector3 position, Color tintColor, float startingAlpha = 0.5f)
	{
		// 1. Get an object from the pool
		AfterImage afterImage = _afterImagePool.Get();

		// 2. Setup it, passing the pool so the object knows where to return
		afterImage.Initialize(sprite, position, tintColor, startingAlpha, _afterImagePool);
	}

	//----POOL CALLBACK METHODS----

	private AfterImage CreatePooledItem() => Instantiate(afterImagePrefab, transform);
	private void OnTakeFromPool(AfterImage afterImage) => afterImage.gameObject.SetActive(true);
	private void OnReturnedToPool(AfterImage afterImage) => afterImage.gameObject.SetActive(false);
	private void OnDestroyPoolObject(AfterImage afterImage) => Destroy(afterImage.gameObject);
}
