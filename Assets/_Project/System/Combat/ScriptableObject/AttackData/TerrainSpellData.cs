using UnityEngine;

[CreateAssetMenu(fileName = "Spell_Data_Terrain_", menuName = "SpellData/TerrainSpellData")]
public class TerrainSpellData : SpellData
{
	[Header("Terrain")]
	public float terrainHpMax = 25f;
	public float terrainDuration = 120f;
}
