using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpellParticleController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Light2D _light;
    private EntityAnimator _entityAnimator;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _light = GetComponent<Light2D>();
        
        _entityAnimator = GetComponentInParent<EntityAnimator>();
        if (_entityAnimator != null) _entityAnimator.OnAnimationCanceled += ClearParticles;
    }

    private void OnDisable()
    {
        if (_entityAnimator != null) _entityAnimator.OnAnimationCanceled -= ClearParticles;
    }

    private void ClearParticles()
    {
        if (_spriteRenderer != null) _spriteRenderer.sprite = null;
        if (_light != null) _light.lightCookieSprite = null;
    }

    private void LateUpdate()
    {
        if (_light.lightCookieSprite != _spriteRenderer.sprite)
            _light.lightCookieSprite = _spriteRenderer.sprite;
    }
}
