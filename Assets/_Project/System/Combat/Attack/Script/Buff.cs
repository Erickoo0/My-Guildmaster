using UnityEngine;

public class Buff : MonoBehaviour
{
    [Header("References")]
    private GameObject _buffReceiver;
    private Health _health;
    private Mana _mana;
    private BuffSpellData.BuffType _buffType;
    public BuffSpellData.BuffType BuffType => _buffType;
    
    [Header("Buff Settings")]
    private float _buffDuration;
    private float _buffAmount;
    private float _buffAmountPerTick;
    private float _buffTickRate = 0.5f;
    private float _buffTickTimer;

    public void Setup(GameObject buffReceiver, BuffSpellData.BuffType buffType, float buffAmount, float buffDuration)
    {
        _buffReceiver = buffReceiver;
        _buffType = buffType;
        _buffDuration = buffDuration;
        _buffAmount = buffAmount;
        
        // Calculate total ticks and amount per tick
        int totalTicks = Mathf.CeilToInt(_buffDuration / _buffTickRate);
        _buffAmountPerTick = _buffAmount / totalTicks;
        
        Destroy(gameObject, _buffDuration);

        HandleBuffType();
    }
    
    private void HandleBuffType()
    {
        // 1. Get stats from the IStatProvider interface (if it exists)
        if (_buffReceiver.TryGetComponent(out IStatProvider statProvider))
        {
            switch (_buffType)
            {
                case BuffSpellData.BuffType.Health:
                    _health = statProvider.EntityHealth;
                    break;
                case BuffSpellData.BuffType.Mana:
                    _mana = statProvider.EntityMana;
                    break;
            }
        } 
        else // 2. If there is no IStatProvider, get the components from the entity directly
        {
            switch (_buffType)
            {
                case BuffSpellData.BuffType.Health:
                    _health = _buffReceiver.GetComponent<Health>();
                    break;
                case BuffSpellData.BuffType.Mana:
                    _mana = _buffReceiver.GetComponent<Mana>();
                    break;
            }
        }
        
        // Safety Warnings
        if (_buffType == BuffSpellData.BuffType.Health && _health == null)
            Debug.LogWarning($"Buff tried to heal, but {_buffReceiver.name} has no Health reference.");
        if (_buffType == BuffSpellData.BuffType.Mana && _mana == null)
            Debug.LogWarning($"Buff tried to restore MP, but {_buffReceiver.name} has no Mana reference.");
    }

    private void Update()
    {
        _buffTickTimer += Time.deltaTime;

        if (_buffTickTimer >= _buffTickRate)
        {
            ApplyTick();
            _buffTickTimer = 0;
        }
    }

    private void ApplyTick()
    {
        switch (_buffType)
        {
            case BuffSpellData.BuffType.Health:
                if (_health != null) _health.HpHealInstant(_buffAmountPerTick);
                break;
            case BuffSpellData.BuffType.Mana:
                if (_mana != null) _mana.MpHealInstant(_buffAmountPerTick);
                break;
        }
    }
}
