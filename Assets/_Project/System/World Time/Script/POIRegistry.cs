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
        return POIList.Where(POI => ids.Contains(POI.ID)).ToList();
    }
}
