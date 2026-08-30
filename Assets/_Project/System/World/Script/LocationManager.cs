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
	Tavern,
	Library,
	Church,
	Haven_Hills,
	Clinic,
	Community_Center,
	Weapon_Shop,
	Armor_Shop,
	Potion_Shop,
	Magic_Shop,
	General_Store,

	// ==========================================
	// ⚔️ GUILDS & FACTIONS (Interiors)
	// ==========================================
	Player_Guild,
	Guild1,
	Guild2,
	Guild3,

	// ==========================================
	// 🏠 PRIVATE RESIDENCES (Interiors)
	// ==========================================
	Player_Home,
	Mai_Home,
	Eric_Home,
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
	Goblin_Forest,
	Forest_Path,
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
