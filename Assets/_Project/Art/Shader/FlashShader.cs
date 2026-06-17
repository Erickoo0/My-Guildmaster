using UnityEngine;

public class FlashShader : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashDecaySpeed = 5f;
    
    [Header("Blink Settings")]
    [SerializeField] private float blinkFrequency = 20f; 
    
    private SpriteRenderer[] _spriteRenderers;
    private MaterialPropertyBlock _materialPropertyBlock;
    
    private float _flashFactor;
    private bool _isBlinking;
    private float _currentAlpha = 1f;
    
    // NEW: A dedicated timer for the blink, replacing Time.time
    private float _blinkTimer; 

    private void Awake() 
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        // 1. Handle Flash Decay
        if (_flashFactor > 0f)
        {
            _flashFactor = Mathf.MoveTowards(_flashFactor, 0f, flashDecaySpeed * Time.deltaTime);
        }
        
        // 2. Handle Blinking Logic
        if (_isBlinking)
        {
            _blinkTimer += Time.deltaTime;
            
            _currentAlpha = (Mathf.FloorToInt(_blinkTimer * blinkFrequency) % 2 == 0) ? 1f : 0f;
            
            if (_flashFactor > 0.5f)
            {
                _currentAlpha = 1f;
            }
        }
        else
        {
            _currentAlpha = 1f;
            _blinkTimer = 0f; // Reset for the next time we get hit
        }

        // 3. Apply everything to the Renderers
        ApplyProperties();
    }

    public void ApplyFlash()
    {
        _flashFactor = 1f;
    }

    public void SetBlinking(bool active)
    {
        _isBlinking = active;
    }
    
    private void ApplyProperties()
    {
        foreach (var sr in _spriteRenderers)
        {
            sr.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_FlashFactor", _flashFactor);
            _materialPropertyBlock.SetFloat("_Alpha", _currentAlpha);
            sr.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}