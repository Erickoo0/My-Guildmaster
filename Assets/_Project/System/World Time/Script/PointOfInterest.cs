using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    private void OnEnable() => POIRegistry.Add(this);
    private void OnDisable() => POIRegistry.Remove(this);

    public string ID;
    public Vector2 Position => transform.position;
    public TeleportFacing faceDirection;
}
