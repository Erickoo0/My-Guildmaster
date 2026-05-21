using UnityEngine;

[CreateAssetMenu(fileName = "ChargeSpellData", menuName = "SpellData/ChargeSpellData")]
public class ChargeSpellData : SpellData
{
    [Header("Movement Settings")] 
    public float windUpDuration = 0.75f;
    public float chargeSpeedMultiplier = 4f;
    public float overshootDistance = 5f;
}
