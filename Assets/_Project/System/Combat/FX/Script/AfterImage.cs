using System;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 4.5f;
    
    private SpriteRenderer _spriteRenderer;
    private Action<AfterImage> _returnToPool;
    private Color _currentColor;

    private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

    // Called by the Manager
    public void Initialize(Sprite sprite, Vector3 position, Color tintColor, float startingAlpha, Action<AfterImage> returnAction)
    {
        _returnToPool = returnAction;
        
        _spriteRenderer.sprite = sprite;
        _currentColor = new Color(tintColor.r, tintColor.g, tintColor.b, startingAlpha);
        _spriteRenderer.color = _currentColor;
        transform.position = position;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _currentColor.a -= _fadeSpeed * Time.deltaTime;
        _spriteRenderer.color = _currentColor;

        if (_currentColor.a <= 0f)
        {
            _returnToPool?.Invoke(this);
        }
    }
}