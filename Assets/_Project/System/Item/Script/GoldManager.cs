using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour, ISaveable
{
   public static GoldManager Instance { get; private set; }

   private int playerGold = 0;
   [SerializeField] private TextMeshProUGUI goldText;

   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }
      
      Instance = this;
      
      goldText.text = playerGold.ToString();
   }
   
   public void AddGold(int amount)
   {
      playerGold += amount;
      goldText.text = playerGold.ToString();
   }
   
   public void RemoveGold(int amount)
   {
      playerGold -= amount;
      goldText.text = playerGold.ToString();
   }

   public void PopulateSaveData(SaveData saveData)
   {
      saveData.currentPlayerGold = playerGold;
   }

   public void LoadFromSaveData(SaveData saveData)
   {
      playerGold = saveData.currentPlayerGold;
   }
}
