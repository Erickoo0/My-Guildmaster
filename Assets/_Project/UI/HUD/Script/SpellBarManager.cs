using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SpellBarManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Image spellBarQIcon;
	[SerializeField] private Image spellBarQCooldown;
	[SerializeField] private TextMeshProUGUI spellBarQText;
	[SerializeField] private Image spellBarEIcon;
	[SerializeField] private Image spellBarECooldown;
	[SerializeField] private TextMeshProUGUI spellBarEText;
	[SerializeField] private Image spellBarRIcon;
	[SerializeField] private Image spellBarRCooldown;
	[SerializeField] private TextMeshProUGUI spellBarRText;
	[SerializeField] private Image spellBarFIcon;
	[SerializeField] private Image spellBarFCooldown;
	[SerializeField] private TextMeshProUGUI spellBarFText;
	[SerializeField] private ControllerPlayer _controllerPlayer;

	private readonly List<TrackedSkillUI> _activeSkillSlots = new List<TrackedSkillUI>();

	private void Start()
	{
		if (_controllerPlayer == null || _controllerPlayer.SkillController.SkillSlots == null) return;

		// Slot 1: Q
		RegisterSkillSlot(1, spellBarQIcon, spellBarQText, spellBarQCooldown, "Q");

		// Slot 2: E
		RegisterSkillSlot(2, spellBarEIcon, spellBarEText, spellBarECooldown, "E");

		// Slot 3: R
		RegisterSkillSlot(3, spellBarRIcon, spellBarRText, spellBarRCooldown, "R");

		// Slot 4: F
		RegisterSkillSlot(4, spellBarFIcon, spellBarFText, spellBarFCooldown, "RMB");
	}

	private void Update()
	{
		if (_activeSkillSlots.Count == 0 || _controllerPlayer == null) return;

		foreach (TrackedSkillUI skillUI in _activeSkillSlots)
		{
			if (skillUI.PlayerSkillState?.SkillDataInstance == null || skillUI.CooldownImage == null) continue;

			string skillID = skillUI.PlayerSkillState.SkillDataInstance.ID;
			float cooldown = skillUI.PlayerSkillState.SkillDataInstance.Cooldown;

			float cooldownPercent = _controllerPlayer.SkillController.GetRemainingCooldownRatio(skillID, cooldown);

			skillUI.CooldownImage.fillAmount = cooldownPercent;
		}
	}

	private void RegisterSkillSlot(int slotIndex, Image slotImage, TextMeshProUGUI slotKeybindText, Image cooldownImage, string keyName)
	{
		// 1. Ensure index exists within skill slots list
		if (_controllerPlayer.SkillController.SkillSlots.Count > slotIndex && _controllerPlayer.SkillController.SkillSlots[slotIndex] != null)
		{
			var spell = _controllerPlayer.SkillController.SkillSlots[slotIndex];
			var spellData = spell.SkillDataInstance;

			// 2. Setup active UI
			if (spellData != null && spellData.Icon != null)
			{
				slotImage.sprite = spellData.Icon;
				slotKeybindText.text = keyName;
				slotImage.enabled = true;

				if (cooldownImage != null)
				{
					cooldownImage.fillAmount = 0f;
					cooldownImage.enabled = true;
				}

				// Add to our tracking list so Update() knows to animate it
				_activeSkillSlots.Add(new TrackedSkillUI
				{
					PlayerSkillState = spell,
					CooldownImage = cooldownImage
				});

				return; // Successfully registered, exit early
			}
		}

		// 3. Fallback: Turn off the slot if there's no skill data or the slot is empty
		slotImage.enabled = false;
		slotKeybindText.text = "";
		if (cooldownImage != null) cooldownImage.enabled = false;
	}

	private class TrackedSkillUI
	{
		public Image CooldownImage;
		public PlayerSkillStateBase PlayerSkillState;
	}
}
