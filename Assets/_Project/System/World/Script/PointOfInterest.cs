using UnityEngine;
public class PointOfInterest : MonoBehaviour
{

	public string ID;
	public GameLocation Location;
	[HierarchySelector(typeof(PointOfInterest))]
	public string TeleportPOI;
	public FacingDirection lookDirection;
	public Vector2 Position => transform.position;
	private void OnEnable() => POIRegistry.Add(this);
	private void OnDisable() => POIRegistry.Remove(this);
}
