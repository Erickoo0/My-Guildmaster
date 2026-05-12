using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCHomeState : BaseNPCHomeState
{
    protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.HomePOIList;
}
