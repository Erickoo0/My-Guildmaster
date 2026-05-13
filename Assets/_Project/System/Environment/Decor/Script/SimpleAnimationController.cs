using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimationController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private SimpleAnimationData animationData;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 10f;
    
    private SpriteRenderer _spriteRenderer;
    private int _currentFrame;
    private float _timer;
    private float _frameDuration;
    private bool _isSwaying;
    private float _nextSwayCooldown;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If we don't have animation data, then don't do anything
        if (animationData == null) return;
        
        _frameDuration = 1f / animationData.fps;

        // Start at frame 0 and pick a random wait time
        _spriteRenderer.sprite = animationData.animationFrames[0];
        ResetSwayTimer();
    }

    private void Update()
    {
        if (animationData == null) return;
        
        if (_isSwaying)
        {
            HandleAnimation();
        }
        else
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
            
            // Check if we've reached the end of the animation'
            if (_currentFrame < animationData.animationFrames.Length)
            {
                _spriteRenderer.sprite = animationData.animationFrames[_currentFrame];
            }
            else
            {
                _isSwaying = false;
                _spriteRenderer.sprite = animationData.animationFrames[0];
                ResetSwayTimer();
            }
        }
    }

    private void HandleIdle()
    {
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


