using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellBarManager : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Image spellBarQIcon;
    [SerializeField] private TextMeshProUGUI spellBarQText;
    [SerializeField] private Image spellBarEIcon;
    [SerializeField] private TextMeshProUGUI spellBarEText;
    [SerializeField] private Image spellBarRIcon;
    [SerializeField] private TextMeshProUGUI spellBarRText;
    [SerializeField] private Image spellBarFIcon;
    [SerializeField] private TextMeshProUGUI spellBarFText;
    [SerializeField] private PlayerController _playerController;

    private void Start()
    {
        if (_playerController == null || _playerController.spellController.SpellSlots == null) return;
        
        // Slot 1: Q
        UpdateSpellSlotUI(1, spellBarQIcon, spellBarQText, "Q");
        
        // Slot 2: E
        UpdateSpellSlotUI(2, spellBarEIcon, spellBarEText, "E");
        
        // Slot 3: R
        UpdateSpellSlotUI(3, spellBarRIcon, spellBarRText, "R");
        
        // Slot 4: F
        UpdateSpellSlotUI(4, spellBarFIcon, spellBarFText, "RMB");
    }

    private void UpdateSpellSlotUI(int slotIndex, Image slotImage, TextMeshProUGUI slotKeybindText, string keyName)
    {
        // 1. Ensure index exists within skill slots list
        if (_playerController.spellController.SpellSlots.Count > slotIndex && _playerController.spellController.SpellSlots[slotIndex] != null)
        {
            // 2. Ensure skill data exists
            var spell = _playerController.spellController.SpellSlots[slotIndex];
            var spellData = spell.GetSpellDataSource();
            
            // 3. Update UI
            if (spellData != null && spellData.Icon != null)
            {
                slotImage.sprite = spellData.Icon;
                slotKeybindText.text = keyName;
                slotImage.enabled = true;
            } 
            else // Turn off the slot if there's no skill data'
            {
                slotImage.enabled = false;
                slotKeybindText.text = "";
            }
        } else // Turn off the slot if there's no skill data'
        {
            slotImage.enabled = false;
            slotKeybindText.text = "";
        }
    }
}
