using System.Collections;
using UnityEngine;

public class SquashAndStretch : MonoBehaviour
{
    [Header("References")] 
    private SpriteRenderer _spriteRenderer;
    [SerializeField] AnimationCurve xCurve;
    [SerializeField] AnimationCurve yCurve;
    private Coroutine activeRoutine;
    
    private float elapsedTime;
    private float totalDuration = 0.2f;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SquishAndSquash()
    {
        // Stop existing coroutine if it's running
        if (activeRoutine != null) StopCoroutine(SquishAndSquashRoutine());
        
        activeRoutine = StartCoroutine(SquishAndSquashRoutine());
    } 
    
    
    private IEnumerator SquishAndSquashRoutine()
    {
        Vector3 originalScale = _spriteRenderer.transform.localScale;
        elapsedTime = 0f;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            float xScale = xCurve.Evaluate(progress);
            float yScale = yCurve.Evaluate(progress);
            
            _spriteRenderer.transform.localScale = new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z);

            yield return null;
        }
        
        _spriteRenderer.transform.localScale = originalScale;
        activeRoutine = null;
    }
}
