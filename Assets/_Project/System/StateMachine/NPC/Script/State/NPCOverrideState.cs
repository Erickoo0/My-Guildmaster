using System;
[Serializable]
public class NPCOverrideState : BaseNPCOverrideWanderState
{
	// Update is called once per frame
	public override void Update()
	{
		base.Update();

		if (!EvaluateRequirements())
		{
			FinishOverride();
		}
	}
}
