using UnityEngine;

public abstract class SkillControllerBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillDataDatabase _skillDatabase;
    [SerializeField] private SkillTreeDatabase _skillTreeDatabase;
    public FirePoint FirePoint { get; private set; }
    public CastBar CastBar { get; private set; }
    
    [Header("Action Settings")]
    [field: SerializeField] public float ActionCooldown { get; private set; } = 0.5f;
    protected float lastActionTime;

    public SkillDataDatabase SkillDatabase => _skillDatabase;
    public SkillTreeDatabase SkillTreeDatabase => _skillTreeDatabase;
    
    protected virtual void Start()
    {
        FirePoint = GetComponentInChildren<FirePoint>();
        CastBar = GetComponent<CastBar>();
    }
    
    public bool CheckActionCooldown() => Time.time >= lastActionTime + ActionCooldown;
    public void SetActionCooldown() => lastActionTime = Time.time;
}
