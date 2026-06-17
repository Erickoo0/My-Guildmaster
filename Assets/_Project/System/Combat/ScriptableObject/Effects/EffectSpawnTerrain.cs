using UnityEngine;

[System.Serializable]
public class EffectSpawnTerrain : Effect
{
    public GameObject terrainPrefab;
    public float terrainDuration = 5f;
    public float terrainHpMax = 50f;

    public override bool Execute(EffectPayload payload)
    {
        if (terrainPrefab == null) return false;
        
        // 1. Spawn the terrain
        GameObject terrainInstance = Object.Instantiate(terrainPrefab, payload.TargetPosition, Quaternion.identity);
        
        // 2. Pass the data to the terrain
        if (terrainInstance.TryGetComponent(out Terrain terrainComponent))
        {
            terrainComponent.Setup(payload.HitDirection, terrainHpMax, terrainDuration);
            return true;
        }
        
        return false;
    }
}
