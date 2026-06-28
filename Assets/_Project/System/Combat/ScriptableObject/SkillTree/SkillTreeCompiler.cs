using UnityEngine;

/// <summary>
/// Resolves the player's skill ledger and compiles
/// a SkillTree + SkillTreeLedger into a fresh SkillDataInstance.
/// </summary>
public class SkillTreeCompiler: MonoBehaviour
{
    public static SkillTreeCompiler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    /// <summary>
    /// Compile the given SkillTree using the SkillTreeLedger
    /// </summary>

    public SkillDataInstance CompileSkillDataInstance(SkillTree skillTree)
    {
        // Safety Check
        if (skillTree == null)
        {
            Debug.LogWarning($"{name}: Cannot compile because SkillTree is null");
            return null;
        }

        // 1. Find the SkillTreeLedger for the given SkillTree from the SkillTreeLedgerController
        SkillTreeLedger skillTreeLedger = SkillTreeLedgerController.Instance?.GetSkillTreeLedger(skillTree.SkillData.ID);
        return CompileSkillDataInstance(skillTree, skillTreeLedger);
    }

    /// <summary>
    /// Gets or creates the player's SkillTreeLedger for the given SkillTree.
    /// </summary>
    public SkillTreeLedger GetOrCreateSkillTreeLedger(SkillTree skillTree)
    {
        if (skillTree == null || skillTree.SkillData == null)
        {
            Debug.LogWarning($"{name}: Cannot create SkillTreeLedger because SkillTree or skillData is null");
            return null;
        }

        return SkillTreeLedgerController.Instance?.GetOrCreateSkillTreeLedger(skillTree.SkillData.ID);
    }
    
    //----PRIVATE COMPILATION METHODS----

    private static SkillDataInstance CompileSkillDataInstance(SkillTree skillTree, SkillTreeLedger skillTreeLedger)
    {
        if (skillTree.SkillData == null)
        {
            Debug.LogWarning($"SkillTreeCompiler: {skillTree.name} has no bound SkillData.");
            return null;
        }
        
        // 1. Create a SkillDataInstance from the SkillData
        SkillDataInstance skillDataInstanceCompiled = skillTree.SkillData.CreateSkillDataInstance();
        if (skillDataInstanceCompiled == null)
        {
            Debug.LogWarning($"SkillTreeCompiler: {skillTree.SkillData.name} failed to create a SkillDataInstance.");
            return null; 
        }
        
        // 2. Apply modifiers from allocated SkillNodes from the given SkillTree + SkilLTreeLedger to the SkillDataInstance
        ApplyAllocatedSkillNodes(skillTree, skillTreeLedger, skillDataInstanceCompiled);
        return skillDataInstanceCompiled;
    }
    
    private static void ApplyAllocatedSkillNodes(SkillTree skillTree, SkillTreeLedger skillTreeLedger, SkillDataInstance skillDataInstanceCompiled)
    {
        // 1. If there are no allocations, return early
        if (skillTreeLedger?.Allocations == null) return;

        // 2. Loop through each allocation in the SkillTreeLedger
        foreach (SkillNodeAllocation allocation in skillTreeLedger.Allocations)
        {
            if (allocation == null || allocation.AllocatedSkillPoints <= 0) continue;

            // 3. Get the SkillNode from the SkillTree using the allocation's SkillNodeID
            SkillNode skillNode = skillTree.GetSkillNodeByID(allocation.SkillNodeID);
            if (skillNode == null)
            {
                Debug.LogWarning($"SkillTreeCompiler: Ledger for '{skillTreeLedger.SkillDataID}' references missing node '{allocation.SkillNodeID}'.", skillTree);
                continue;
            }

            // 4. Apply modifiers from the SkillNode to the SkillDataInstance
            ApplySkillNodeModifiers(skillNode, allocation.AllocatedSkillPoints, skillDataInstanceCompiled);
        }
    }
    
    private static void ApplySkillNodeModifiers(SkillNode skillNode, int pointsAllocated, SkillDataInstance skillDataInstanceCompiled)
    {
        if (skillNode.Modifiers == null) return;

        for (int i = 0; i < pointsAllocated; i++)
            foreach (ModifierSkillBase modifier in skillNode.Modifiers)
                if (modifier != null) modifier.ApplyModifier(skillDataInstanceCompiled);
    }
}
