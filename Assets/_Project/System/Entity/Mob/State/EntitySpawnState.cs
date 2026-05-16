using UnityEngine;

[System.Serializable]
public class EntitySpawnState : BaseSpawnState
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnFXPrefab;
    [SerializeField] private float spawnDuration = 2f;

    private float _timer;

    public override void Enter()
    {
        controller.EntityMover.SetMoveDirection(Vector2.zero);
        
        _timer = spawnDuration;

        if (spawnFXPrefab != null)
        {
            GameObject spawnFxInstance = Object.Instantiate(spawnFXPrefab, controller.transform.position, Quaternion.identity);
            if (spawnFxInstance.TryGetComponent(out SpawnFX spawnFx))
                spawnFx.SetupSpawnFX(spawnDuration);
        }
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;
        
        if (_timer <= 0)
        {
            stateMachine.ChangeState(controller.WanderState);
        }
    }
    
    public override void PhysicsUpdate() {}
    public override void HandleInput() { }
    public override void Exit() { }
    
}
