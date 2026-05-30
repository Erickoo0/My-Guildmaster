using UnityEngine;
using System.Collections.Generic;

public static class POIRegistry
{
    public static readonly Dictionary<string, PointOfInterest> POIRegistryDict = new Dictionary<string, PointOfInterest>();

    public static void Add(PointOfInterest POI)
    {
        if (string.IsNullOrEmpty(POI.ID)) return;

        if (POIRegistryDict.ContainsKey(POI.ID))
        {
            Debug.LogWarning($"POI Registry: Duplicate ID '{POI.ID}'");
            return;
        }
        
        POIRegistryDict.Add(POI.ID, POI);
    }
    
    public static void Remove(PointOfInterest POI)
    {
        if (string.IsNullOrEmpty(POI.ID)) return;
        
        if (POIRegistryDict.TryGetValue(POI.ID, out PointOfInterest registeredPoi) && registeredPoi == POI)
        {
            POIRegistryDict.Remove(POI.ID);
        }
    }

    public static List<PointOfInterest> GetPOIByIDs(List<string> ids)
    {
        // 1. Create an empty container to hold POIs we find
        List<PointOfInterest> orderedResults = new List<PointOfInterest>();

        // 2.  Loop through the list of IDs in a specific list assigned
        // Foreach respects the order of the list, so we can use it to find POIs in the correct order
        foreach (string id in ids)
        {
            // Safety CHeck
            if (string.IsNullOrEmpty(id)) continue;
            
            // 3. Search the "Master Registry" (POIList) for a POI that has a matching ID string.
            if (POIRegistryDict.TryGetValue(id, out PointOfInterest foundPOI))
            {
                orderedResults.Add(foundPOI);
            }
            else
            {
                Debug.LogWarning($"POI Registry: Could not find POI with ID '{id}'");
            }
        }
        
        // Return the ordered list of POIs
        return orderedResults;    
    }

    public static PointOfInterest GetPOIByID(string ID)
    {
        if (string.IsNullOrEmpty(ID)) return null;
        
        if (POIRegistryDict.TryGetValue(ID, out PointOfInterest foundPOI))
            return foundPOI;
        
        Debug.LogWarning($"POI Registry: Could not find POI with ID '{ID}'");
        return null;   
    }
}
