using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float hpBase = 100f;
    [SerializeField] private float hpPerLvl = 10f;

    public float HpMax;
    private float _hpCurrent;

    [Header("References")] 
    [Tooltip("The actual object the health component belongs to")] 
    [SerializeField] private GameObject entityRoot;

    [Header("Behavior Settings")] 
    [SerializeField] private bool destroyOnDeath = true;
    
    private bool _isDead = false;
    
    public event Action OnHpUpdated;
    public event Action OnDeath;
    
    // Health Property
    public float HpCurrent
    {
        get => _hpCurrent;
        set
        {
            float hpPrevious = _hpCurrent;
            _hpCurrent = Mathf.Clamp(value, 0, HpMax);

            if (!Mathf.Approximately(_hpCurrent, hpPrevious))
            {
                float difference = _hpCurrent - hpPrevious;
                int differenceRounded = Mathf.RoundToInt(difference);
                EventBus.RequestFloatingText(differenceRounded, transform.position);
                
                OnHpUpdated?.Invoke();
            }
            
            if (_hpCurrent <= 0 && !_isDead) SetDead();
        }
    }
    
    private void Awake()
    {
        if (entityRoot == null) entityRoot = gameObject;
        
        // Default initialization
        HpMax = hpBase;
        HpCurrent = HpMax;
    }
    
    public void RecalculateMaxHp(int currentLevel)
    {
        HpMax = hpBase + (currentLevel - 1) * hpPerLvl;
        _hpCurrent = HpMax; 
        OnHpUpdated?.Invoke();
    }
    
    public void HpHealInstant(float hpHealAmount)
    {
        HpCurrent += hpHealAmount;
    }

    private void SetDead()
    {
        _isDead = true;
        OnDeath?.Invoke(); 
        EventBus.RequestEntityDeathUpdate(entityRoot); 
        if (destroyOnDeath) Destroy(entityRoot);
    }
}