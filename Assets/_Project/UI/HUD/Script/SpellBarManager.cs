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

    private void awake()
    {
        //Sprite spellIconQ = _playerController.SpellSlots[0];
    }
}
