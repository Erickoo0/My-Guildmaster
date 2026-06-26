using UnityEngine;

public class SkillTreeLedgerTester : MonoBehaviour
{
	[Header("Test Data")]
	[SerializeField] private SkillTree skillTree;

	[Header("Runtime Ledger")]
	[SerializeField] private string testNodeID;

	private SkillTreeLedger ledger;

	private void Start()
	{
		if (skillTree == null || skillTree.SkillData == null)
		{
			Debug.LogWarning($"{name}: Skill tree or bound SkillData is missing.");
			return;
		}

		ledger = new SkillTreeLedger(skillTree.SkillData.ID);

		Debug.Log($"Created ledger for skill: {ledger.SpellDataID}");
	}

	[ContextMenu("Try Allocate Test Node")]
	private void TryAllocateTestNode()
	{
		if (skillTree == null || ledger == null)
		{
			Debug.LogWarning($"{name}: Missing skill tree or ledger.");
			return;
		}

		bool success = skillTree.TrySkillPointAllocation(testNodeID, ledger);
		int allocatedPoints = ledger.GetAllocatedSkillPoints(testNodeID);

		Debug.Log($"Allocate {testNodeID}: {success}. Current Points: {allocatedPoints}");
	}

	[ContextMenu("Try Refund Test Node")]
	private void TryRefundTestNode()
	{
		if (skillTree == null || ledger == null)
		{
			Debug.LogWarning($"{name}: Missing skill tree or ledger.");
			return;
		}

		bool success = skillTree.TrySkillPointRefund(testNodeID, ledger);
		int allocatedPoints = ledger.GetAllocatedSkillPoints(testNodeID);

		Debug.Log($"Refund {testNodeID}: {success}. Current Points: {allocatedPoints}");
	}

	[ContextMenu("Print Total Allocated Points")]
	private void PrintTotalAllocatedPoints()
	{
		if (ledger == null)
		{
			Debug.LogWarning($"{name}: Ledger is missing.");
			return;
		}

		Debug.Log($"Total allocated points for {ledger.SpellDataID}: {ledger.GetTotalAllocatedSkillPoints()}");
	}
}
