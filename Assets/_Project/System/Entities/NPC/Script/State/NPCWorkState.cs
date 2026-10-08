using System;
using System.Collections.Generic;
[Serializable]
public class NPCWorkState : BaseNPCWorkState
{
	protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.WorkPOIList;
}
