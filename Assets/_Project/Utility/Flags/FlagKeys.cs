public static class FlagKeys
{
	// Returns true or false
	public enum GameFlag
	{
		// NPC & DIALOGUE
		NPC_Met_Mai,
		NPC_Met_Assistant,
    
		// SPELLS
		Spell_Unlocked_Fireball,
		Spell_Unlocked_Lightning_Orb,
		Spell_Unlocked_Ice_Wall,
		Spell_Unlocked_Heal,
    
		// WORLD & TIME
		Event_Bridge_Repaired,
		Season_First_Winter_Reached
	}

	// Returns an int
	public enum GameStat
	{
		SkyTower_HighestFloorReached,
		Player_CurrentLevel
	}
}

