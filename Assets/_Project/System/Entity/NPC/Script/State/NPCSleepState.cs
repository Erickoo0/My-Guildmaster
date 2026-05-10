using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCSleepState : BaseNPCSleepState
{
    protected override List<string> GetPOITargetIDs() => controller.NpcScheduleData.SleepPOIList;
}

