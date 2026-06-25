
using UnityEngine;

public interface ISaveable
{
    // Passes the SaveData box to the script so it can put its data inside
    void PopulateSaveData(SaveData saveData);

    // Passes the filled SaveData box back to the script so it can extract its data
    void LoadFromSaveData(SaveData saveData);
}