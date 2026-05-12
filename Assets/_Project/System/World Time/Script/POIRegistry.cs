using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class POIRegistry
{
    public static readonly List<PointOfInterest> POIList = new List<PointOfInterest>();
    
    public static void Add(PointOfInterest POI) => POIList.Add(POI);
    public static void Remove(PointOfInterest POI) => POIList.Remove(POI);

    public static List<PointOfInterest> GetPOIByIDs(List<string> ids)
    {
        // 1. Create an empty container to hold POIs we find
        List<PointOfInterest> orderedResults = new List<PointOfInterest>();

        // 2.  Loop through the list of IDs in a specific list assigned
        // Foreach respects the order of the list, so we can use it to find POIs in the correct order
        foreach (string id in ids)
        {
            // 3. Search the "Master Registry" (POIList) for a POI that has a matching ID string.
            PointOfInterest found = POIList.FirstOrDefault(p => p.ID == id);
        
            if (found != null)
            {
                orderedResults.Add(found);
            }
            else
            {
                Debug.LogWarning($"POI Registry: Could not find POI with ID '{id}'");
            }
        }
        
        // Return the ordered list of POIs
        return orderedResults;    
    }
}
