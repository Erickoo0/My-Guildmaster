using UnityEngine;

public class Buff : MonoBehaviour
{
    [Header("References")]
    private GameObject _receiver;
    private Health _health;
    private Mana _mana;
    public BuffType Type { get; private set; }

    [Header("Buff Settings")]
    private float _duration;
    private float _amount;
    private float _amountPerTick;
    private readonly float _tickRate = 0.5f;
    private float _tickTimer;

    public void Setup(GameObject receiver, BuffType type, float amount, float duration)
    {
        _receiver = receiver;
        Type = type;
        _duration = duration;
        _amount = amount;
        
        // Calculate total ticks and Amount per tick
        int totalTicks = Mathf.CeilToInt(_duration / _tickRate);
        _amountPerTick = _amount / totalTicks;
        
        Destroy(gameObject, _duration);

        HandleBuffType();
    }
    
    private void HandleBuffType()
    {
        // 1. Get stats from the IStatProvider interface (if it exists)
        if (_receiver.TryGetComponent(out IStatProvider statProvider))
        {
            switch (Type)
            {
                case BuffType.Health:
                    _health = statProvider.EntityHealth;
                    break;
                case BuffType.Mana:
                    _mana = statProvider.EntityMana;
                    break;
            }
        } 
        else // 2. If there is no IStatProvider, get the components from the entity directly
        {
            switch (Type)
            {
                case BuffType.Health:
                    _health = _receiver.GetComponent<Health>();
                    break;
                case BuffType.Mana:
                    _mana = _receiver.GetComponent<Mana>();
                    break;
            }
        }
        
        // Safety Warnings
        if (Type == BuffType.Health && _health == null)
            Debug.LogWarning($"Buff tried to heal, but {_receiver.name} has no Health reference.");
        if (Type == BuffType.Mana && _mana == null)
            Debug.LogWarning($"Buff tried to restore MP, but {_receiver.name} has no Mana reference.");
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= _tickRate)
        {
            ApplyTick();
            _tickTimer = 0;
        }
    }

    private void ApplyTick()
    {
        switch (Type)
        {
            case BuffType.Health:
                if (_health != null) _health.HpHealInstant(_amountPerTick);
                break;
            case BuffType.Mana:
                if (_mana != null) _mana.MpHealInstant(_amountPerTick);
                break;
        }
    }
}
