using UnityEngine;

[CreateAssetMenu(fileName = "Spell_Data_Buff_", menuName = "SpellData/BuffSpellData")]
public class BuffSpellData : SpellData
{
	public enum BuffType { Health, Mana }
	public BuffType buffType;
	public float buffAmount = 30f;
	public float buffDuration = 2f;
}
