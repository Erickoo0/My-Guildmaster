using UnityEngine;

public enum GameLocation
{
    Town,
    GoblinForest,
    Library,
    Guild1,
    PlayerGuild,
    Church
}

/// <summary>
/// Handles the current location of the player
/// </summary>
public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    public GameLocation CurrentLocation { get; private set;  }
    
    public event System.Action<GameLocation> OnLocationChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    // Called by teleporters when the player changes location
    public void UpdateLocation(GameLocation newLocation)
    {
        if (CurrentLocation == newLocation) return;
        
        CurrentLocation = newLocation;
        OnLocationChanged?.Invoke(CurrentLocation);
        Debug.Log($"Location changed to {CurrentLocation}");
    }
}
