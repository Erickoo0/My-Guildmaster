using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCHobbyState : BaseNPCHobbyState
{
    protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.HobbyPOIList;
}
