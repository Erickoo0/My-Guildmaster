using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "FILENAME", menuName = "Quest/Quest", order = 0)]
public class QuestSo : ScriptableObject
{
    [SerializeField] private string questID;
    [SerializeField] private string questName;
    [SerializeField] private string questDescription;
    
    [SerializeField, SerializeReference, SubclassSelector] private List<QuestObjectiveBase> questObjectives = new List<QuestObjectiveBase>();
    
    public string QuestID => questID;
    public string QuestName => questName;
    public string QuestDescription => questDescription;
    public List<QuestObjectiveBase> QuestObjectives => questObjectives;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically sync ID with File Name
        string path = AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (questID != fileName)
            {
                questID = fileName;
                // Mark as dirty to ensure the change is saved
                EditorUtility.SetDirty(this);
            }
        }
    }
#endif
}