using UnityEngine;
using System.Collections;

public class InvulnerableShader : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private Coroutine _flashRoutine;
    
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    public void ApplyFlash(float duration)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(BlinkRoutine(duration));
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        float timer = 0f;
        float interval = 0.1f;

        while (timer < duration)
        {
            float current = _material.GetFloat(FlashAmount);
            _material.SetFloat(FlashAmount, current == 0 ? 1 : 0);
            
            yield return new WaitForSeconds(interval);
            timer += interval;
        }
        
        _material.SetFloat(FlashAmount, 0);
    }
}
