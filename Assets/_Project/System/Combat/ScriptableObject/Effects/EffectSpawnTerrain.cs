using UnityEngine;

[System.Serializable]
public class EffectSpawnTerrain : Effect
{
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public float Duration { get; private set; } = 5f;
    [field: SerializeField] public float HpMax { get; private set; } = 50f;

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
