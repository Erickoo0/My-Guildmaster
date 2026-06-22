using UnityEngine;

public abstract class BaseSpellController : MonoBehaviour
{
    [Header("References")]
    public SpellDataDatabase globalSpellDatabase;
    [HideInInspector] public FirePoint firePoint;
    [HideInInspector] public CastBar castBar;
    
    [Header("Action Settings")]
    [field: SerializeField] public float ActionCooldown { get; private set; } = 0.5f;
    protected float lastActionTime;

    protected virtual void Start()
    {
        firePoint = GetComponentInChildren<FirePoint>();
        castBar = GetComponent<CastBar>();
    }
    
    public bool CheckActionCooldown() => Time.time >= lastActionTime + ActionCooldown;
    public void SetActionCooldown() => lastActionTime = Time.time;
}
