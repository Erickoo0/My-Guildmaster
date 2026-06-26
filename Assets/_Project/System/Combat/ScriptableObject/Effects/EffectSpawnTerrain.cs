using UnityEngine;

[System.Serializable]
public class EffectSpawnTerrain : Effect
{
    public GameObject Prefab;
    public float Duration = 5f;
    public float HpMax = 50f;

    public override bool Execute(EffectPayload payload)
    {
        if (Prefab == null) return false;
        
        // 1. Spawn the terrain
        GameObject terrainInstance = Object.Instantiate(Prefab, payload.TargetPosition, Quaternion.identity);
        
        // 2. Pass the data to the terrain
        if (terrainInstance.TryGetComponent(out Terrain terrainComponent))
        {
            terrainComponent.Setup(payload.HitDirection, HpMax, Duration);
            return true;
        }
        
        return false;
    }
    
    public override Effect Clone()
    {
        return new EffectSpawnTerrain
        {
            Prefab = Prefab,
            Duration = Duration,
            HpMax = HpMax
        };
    }
}
