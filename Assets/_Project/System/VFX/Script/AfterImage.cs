using System;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 4.5f;
    
    private SpriteRenderer _spriteRenderer;
    private IObjectPool<AfterImage> _pool;
    
    private Color _currentColor;
    

    private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

    // Called by the Manager
    public void Initialize(Sprite sprite, Vector3 position, Color tintColor, float startingAlpha, IObjectPool<AfterImage> pool)
    {
        _pool = pool;
        _spriteRenderer.sprite = sprite;
        _currentColor = new Color(tintColor.r, tintColor.g, tintColor.b, startingAlpha);
        _spriteRenderer.color = _currentColor;
        transform.position = position;
    }

    private void Update()
    {
        _currentColor.a -= _fadeSpeed * Time.deltaTime;
        _spriteRenderer.color = _currentColor;

        // If the alpha is zero, return to the pool
        if (_currentColor.a <= 0f && _pool != null)
            _pool.Release(this);
    }
}