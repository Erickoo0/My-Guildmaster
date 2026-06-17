using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

public static class HiearchySorter
{
    [MenuItem("GameObject/Sort Children Alphabetically")]
    private static void SortChildrenAlphabetically(MenuCommand menuCommand)
    {
        // 1. Get the selected parent GameObject
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.transform.childCount == 0)
        {
            Debug.LogWarning("Please select a parent GameObject that has child objects");
            return;
        }
        
        // 2. Save the position of each child object in a list
        // for Undo functionality
        Transform[] children = new Transform[parent.transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = parent.transform.GetChild(i);
        Undo.RegisterCompleteObjectUndo(parent, "Sort Children Alphabetically");
        
        // 3. Sort the list of children alphabetically
        List<Transform> sortedChildren = children.OrderBy(child => child.name).ToList();
        
        // 4. Reorder hierarchy using the new sorted list
        for (int i = 0; i < sortedChildren.Count; i++)
            sortedChildren[i].SetSiblingIndex(i);
        
        Debug.Log($"Successfully sorted {sortedChildren.Count} POIs under '{parent.name}'!");
    }
}
