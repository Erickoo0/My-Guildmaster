using System;
using System.Collections.Generic;
[Serializable]
public class NPCSleepState : BaseNPCSleepState
{
	protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.SleepPOIList;
}
