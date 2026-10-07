using System;
using System.Collections.Generic;
[Serializable]
public class NPCHobbyState : BaseNPCHobbyState
{
	protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.HobbyPOIList;
}
