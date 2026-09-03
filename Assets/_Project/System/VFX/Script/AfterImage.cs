using UnityEngine;
using UnityEngine.Pool;
/// <summary>
/// Handles the logic of a single AfterImage
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour
{
	[SerializeField] private float _fadeSpeed = 4.5f;

	private Color _currentColor;
	private IObjectPool<AfterImage> _pool;

	private SpriteRenderer _spriteRenderer;


	private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

	private void Update()
	{
		_currentColor.a -= _fadeSpeed*Time.deltaTime;
		_spriteRenderer.color = _currentColor;

		// If the alpha is zero, return to the pool
		if (_currentColor.a <= 0f && _pool != null)
			_pool.Release(this);
	}

	// Called by the Manager
	public void Initialize(Sprite sprite, Vector3 position, Color tintColor, float startingAlpha, IObjectPool<AfterImage> pool)
	{
		_pool = pool;
		_spriteRenderer.sprite = sprite;
		_currentColor = new Color(tintColor.r, tintColor.g, tintColor.b, startingAlpha);
		_spriteRenderer.color = _currentColor;
		transform.position = position;
	}
}
