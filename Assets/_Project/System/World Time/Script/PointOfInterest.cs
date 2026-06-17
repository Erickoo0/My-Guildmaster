using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    private void OnEnable() => POIRegistry.Add(this);
    private void OnDisable() => POIRegistry.Remove(this);

    public string ID;
    public GameLocation Location;
    [HierarchySelector(typeof(PointOfInterest))]
    public string TeleportPOI;
    public Vector2 Position => transform.position;
    public FacingDirection lookDirection;
}
