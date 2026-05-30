using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCWorkState : BaseNPCWorkState
{
    protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.WorkPOIList;
}
