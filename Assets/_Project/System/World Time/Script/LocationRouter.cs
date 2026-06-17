using System.Collections.Generic;
using UnityEngine;

public static class LocationRouter
{
    /// <summary>
    /// Calculates the shortest path of teleporters POIs between two locations and returns the NEXT teleporter POI the NPC should walk to
    /// </summary>   
    public static PointOfInterest GetNextTransitNode(GameLocation startLocation, GameLocation targetLocation)
    {
        // If we are already in the correct ozne, we dont need a teleporter POI!
        if (startLocation == targetLocation) return null;

        // 1. Create a To Check List
        Queue<(GameLocation location, PointOfInterest firstTransit)> queue = new Queue<(GameLocation, PointOfInterest)>();
        
        // 2. Save a list of Teleporter POIs we have already checked
        HashSet<GameLocation> visited = new HashSet<GameLocation>();

        // 3. Add the starting location to the queue and list
        queue.Enqueue((startLocation, null));
        visited.Add(startLocation);

        // 4. Loop through the queue as long as there are POIs to check
        while (queue.Count > 0)
        {
            // Pull the next POI off the top of the To-Check list, and read its GameLocation
            var (currentLoc, firstTransit) = queue.Dequeue();

            // Did we find the target location?
            // If so, return the POI we found.
            if (currentLoc == targetLocation)
                return firstTransit;

            // We haven't found the target yet. Look at every single POI in the whole game.
            foreach (var kvp in POIRegistry.POIRegistryDict)
            {
                PointOfInterest poi = kvp.Value;
                
                // Filter for only POIs that are in the current location, and that have a TeleportPOI ID
                if (poi.Location == currentLoc && !string.IsNullOrEmpty(poi.TeleportPOI))
                {
                    // Where does the POI lead to?
                    PointOfInterest targetDoor = POIRegistry.GetPOIByID(poi.TeleportPOI);
                    
                    // If we haven't already checked the location this POI leads to
                    if (targetDoor != null && !visited.Contains(targetDoor.Location))
                    {
                        // Mark the POI as visited
                        visited.Add(targetDoor.Location);
                        
                        // Keep track of the very first door. If this is the first step, log it. Otherwise, pass it down.
                        PointOfInterest pathInitiator = (firstTransit == null) ? poi : firstTransit;
                        
                        // Add the new room to the bottom of the To-Check list
                        queue.Enqueue((targetDoor.Location, pathInitiator));
                    }
                }
            }
        }

        Debug.LogWarning($"LocationRouter: Could not find any sequence of doors to get from {startLocation} to {targetLocation}. Are they connected?");
        return null;
    }
}