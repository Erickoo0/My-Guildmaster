using UnityEngine;
using System.Collections.Generic;

public abstract class Requirement : ScriptableObject
{
    public abstract bool IsMet();
}
