public static class FlagKeys
{
	// Returns true or false
	public enum GameFlag
	{
		// NPC & DIALOGUE
		NPC_Met_Mai,
		NPC_Met_Edwin,
		NPC_Met_Stob,
		NPC_Met_Jane,
		NPC_Met_Eric,
    
		// SPELLS
		Spell_Unlocked_Fireball,
		Spell_Unlocked_Lightning_Orb,
		Spell_Unlocked_Ice_Wall,
		Spell_Unlocked_Heal,
    
		// WORLD & TIME
		Event_Bridge_Repaired,
		Season_First_Winter_Reached,
	}

	// Returns an int
	public enum GameStat
	{
		// NPC & DIALOGUE
		NPCs_Met,
		
		// QUESTS
		Quest_Tutorial,
		
		// WORLD
		SkyTower_HighestFloorReached,
		
		// STATS
		Player_CurrentLevel
	}
}

