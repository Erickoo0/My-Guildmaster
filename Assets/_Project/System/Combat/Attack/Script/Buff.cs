using UnityEngine;

public class Buff : MonoBehaviour
{
    private GameObject _buffReceiver;
    private Health _health;
    private Mana _mana;
    private BuffSpellData.BuffType _buffType;
    
    private float _buffDuration;
    private float _buffAmountPerSecond;

    public void Setup(GameObject buffReceiver, BuffSpellData.BuffType buffType, float buffAmount, float buffDuration)
    {
        _buffReceiver = buffReceiver;
        _buffType = buffType;
        _buffDuration = buffDuration;
        
        // Calculate how much to apply per second
        _buffAmountPerSecond = buffAmount / _buffDuration;
        
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
        if (_health != null)
            _health.HpCurrent += _buffAmountPerSecond * Time.deltaTime;
        else if (_mana != null)
            _mana.MpCurrent += _buffAmountPerSecond * Time.deltaTime; 
    }
}
