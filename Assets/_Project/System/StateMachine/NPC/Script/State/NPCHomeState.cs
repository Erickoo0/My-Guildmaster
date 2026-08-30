using System;
using System.Collections.Generic;
[Serializable]
public class NPCHomeState : BaseNPCHomeState
{
	protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.HomePOIList;
}
