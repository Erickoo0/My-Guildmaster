using System;
using UnityEngine;
public enum GameLocation
{
	// ==========================================
	// 🌳 OUTDOORS & OVERWORLD ZONES
	// ==========================================
	SpellHarbor,
	SpellHarbor_Exterior,

	// ==========================================
	// 🍺 PUBLIC BUILDINGS & SHOPS (Interiors)
	// ==========================================
	Tavern_Interior,
	Library_Interior,
	Church_Interior,
	Haven_Hills_Interior,
	Clinic_Interior,
	Community_Center_Interior,
	Weapon_Shop_Interior,
	Armor_Shop_Interior,
	Potion_Shop_Interior,
	Magic_Shop_Interior,
	General_Store_Interior,

	// ==========================================
	// ⚔️ GUILDS & FACTIONS (Interiors)
	// ==========================================
	Guild_Player_Interior,
	Guild_Lioness_Interior,
	Guild_Adventure_Interior,
	Guild_Rainfall_Interior,

	// ==========================================
	// 🏠 PRIVATE RESIDENCES (Interiors)
	// ==========================================
	Home_Player_Interior,
	Home_Mai_Interior,
	Home_Eric_Interior,
	NPC2_Home,
	NPC3_Home,
	NPC4_Home,
	NPC5_Home,
	NPC6_Home,
	NPC7_Home,
	NPC8_Home,
	NPC9_Home,
	NPC10_Home,

	// ==========================================
	// 💀 HOSTILE ZONES & DUNGEONS
	// ==========================================
	Misten_Forest,
	Misten_Forest_Path,
	Abandoned_Mine,

	Dungeon1_Floor1,
	Dungeon1_Floor2,
	Dungeon1_BossRoom,

	Dungeon2_Floor1,
	Dungeon2_Floor2,
	Dungeon2_BossRoom
}

/// <summary>
/// Handles the current location of the player
/// </summary>
public class LocationManager : MonoBehaviour
{
	public static LocationManager Instance { get; private set; }

	public GameLocation CurrentLocation { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		Instance = this;
	}

	public event Action<GameLocation> OnLocationChanged;

	// Called by teleporters when the player changes location
	public void UpdateLocation(GameLocation newLocation)
	{
		if (CurrentLocation == newLocation) return;

		CurrentLocation = newLocation;
		OnLocationChanged?.Invoke(CurrentLocation);
		Debug.Log($"Location changed to {CurrentLocation}");
	}
}
