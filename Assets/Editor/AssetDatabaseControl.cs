using UnityEditor;

public class AssetDatabaseControl
{
    [MenuItem("Tools/DANGER - Stop Refreshing")]
    public static void Stop() => AssetDatabase.StartAssetEditing();

    [MenuItem("Tools/RESUME Refreshing")]
    public static void Resume() => AssetDatabase.StopAssetEditing();
}