using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimationController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private SimpleAnimationData animationData;
	[SerializeField] private float minIdleTime = 2f;
	[SerializeField] private float maxIdleTime = 10f;
	private int _currentFrame;
	private float _frameDuration;

	// Flag to determine if we should bypass idle timers entirely
	private bool _isConstantPlay;
	private bool _isSwaying;
	private float _nextSwayCooldown;

	private SpriteRenderer _spriteRenderer;
	private float _timer;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		// If we don't have animation data, then don't do anything
		if (animationData == null)
		{
			enabled = false; // turn off the component
			return;
		}

		_frameDuration = 1f/animationData.fps;
		_spriteRenderer.sprite = animationData.animationFrames[0];

		// Check if both idle values are exactly 0 (or practically 0)
		_isConstantPlay = Mathf.Approximately(minIdleTime, 0f) && Mathf.Approximately(maxIdleTime, 0f);

		if (_isConstantPlay)
		{
			_isSwaying = true;
			_timer = 0;
		} else
		{
			ResetSwayTimer();
		}
	}

	private void Update()
	{
		if (_isSwaying)
		{
			HandleAnimation();
		} else
		{
			HandleIdle();
		}
	}

	private void HandleAnimation()
	{
		_timer += Time.deltaTime;
		if (_timer >= _frameDuration)
		{
			_timer -= _frameDuration;
			_currentFrame++;

			// Check if we've reached the end of the animation
			if (_currentFrame < animationData.animationFrames.Length)
			{
				_spriteRenderer.sprite = animationData.animationFrames[_currentFrame];
			} else
			{
				// Loop behavior depends on constant play flag
				if (_isConstantPlay)
				{
					_currentFrame = 0;
					_spriteRenderer.sprite = animationData.animationFrames[0];
				} else
				{
					_isSwaying = false;
					_spriteRenderer.sprite = animationData.animationFrames[0];
					ResetSwayTimer();
				}
			}
		}
	}

	private void HandleIdle()
	{
		// Safety guard: if constant play somehow reaches here, force it back out
		if (_isConstantPlay)
		{
			_isSwaying = true;
			_timer = 0;
			_currentFrame = 0;
			return;
		}

		_timer += Time.deltaTime;
		if (_timer >= _nextSwayCooldown)
		{
			_isSwaying = true;
			_timer = 0;
			_currentFrame = 0;
		}
	}

	private void ResetSwayTimer()
	{
		_timer = 0;
		_nextSwayCooldown = Random.Range(minIdleTime, maxIdleTime);
	}
}
