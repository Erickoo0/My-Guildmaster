using UnityEngine;

public class FlashExplosionFX : MonoBehaviour
{
    [SerializeField] private string animationName = "FlashExplosionFX";

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    public void SetupExplosion(float gameplayRadius)
    {
        
        // 2. Force the animation to play from the beginning
        _animator.Play(animationName, -1, 0f);
    }

    // The animator will call this method on the last frame
    public void DestroySelf() => Destroy(gameObject);
}
