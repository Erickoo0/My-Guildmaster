using UnityEngine;
using System;

public class HierarchySelectorAttribute : PropertyAttribute
{
    public Type TargetType { get; private set; }

    public HierarchySelectorAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
