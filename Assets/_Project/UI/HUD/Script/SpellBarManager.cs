using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SpellBarManager : MonoBehaviour
{
	[Header("Player Reference")]
	[SerializeField] private ControllerPlayer _controllerPlayer;

	[Header("Q Slot")]
	[SerializeField] private Image _qIcon;
	[SerializeField] private TextMeshProUGUI _qText;
	[SerializeField] private Image _qCooldown;
	[SerializeField] private TextMeshProUGUI _qCooldownText;

	[Header("E Slot")]
	[SerializeField] private Image _eIcon;
	[SerializeField] private TextMeshProUGUI _eText;
	[SerializeField] private Image _eCooldown;
	[SerializeField] private TextMeshProUGUI _eCooldownText;

	[Header("R Slot")]
	[SerializeField] private Image _rIcon;
	[SerializeField] private TextMeshProUGUI _rText;
	[SerializeField] private Image _rCooldown;
	[SerializeField] private TextMeshProUGUI _rCooldownText;

	[Header("F Slot")]
	[SerializeField] private Image _fIcon;
	[SerializeField] private TextMeshProUGUI _fText;
	[SerializeField] private Image _fCooldown;
	[SerializeField] private TextMeshProUGUI _fCooldownText;

	private readonly List<TrackedSkillUI> _activeSkillSlots = new List<TrackedSkillUI>();

	private IEnumerator Start()
	{

		// Wait 1 frame to guarantee SkillControllerPlayer has finished its Start() method
		yield return null;

		if (_controllerPlayer == null || _controllerPlayer.SkillController.SkillSlots == null) yield break;

// Slot 1: Q
		RegisterSkillSlot(1, _qIcon, _qText, _qCooldown, _qCooldownText, "Q");

		// Slot 2: E
		RegisterSkillSlot(2, _eIcon, _eText, _eCooldown, _eCooldownText, "E");

		// Slot 3: R
		RegisterSkillSlot(3, _rIcon, _rText, _rCooldown, _rCooldownText, "R");

		// Slot 4: F
		RegisterSkillSlot(4, _fIcon, _fText, _fCooldown, _fCooldownText, "RMB");
	}

	private void Update()
	{
		if (_activeSkillSlots.Count == 0 || _controllerPlayer == null) return;

		// Loop through all active skill slots and update their cooldowns
		foreach (TrackedSkillUI skillUI in _activeSkillSlots)
		{
			// Safety Check
			if (skillUI.PlayerSkillState?.SkillDataInstance == null || skillUI.CooldownImage == null) continue;

			string skillID = skillUI.PlayerSkillState.SkillDataInstance.ID;
			float cooldown = skillUI.PlayerSkillState.SkillDataInstance.Cooldown;

			// Update cooldown swipe
			float cooldownPercent = _controllerPlayer.SkillController.GetRemainingCooldownRatio(skillID, cooldown);
			skillUI.CooldownImage.fillAmount = cooldownPercent;

			// Update cooldown text
			if (skillUI.CooldownText != null)
			{
				float remainingCooldown = _controllerPlayer.SkillController.GetRemainingCooldownTime(skillID, cooldown);

				if (remainingCooldown > 0)
				{
					// CeilToInt ensures that 0.1 sec still shows as 1 sec
					skillUI.CooldownText.text = Mathf.CeilToInt(remainingCooldown).ToString();
					skillUI.CooldownText.enabled = true;
				} else
				{
					skillUI.CooldownText.enabled = false;
				}
			}

		}
	}

	private void RegisterSkillSlot(int slotIndex, Image slotImage, TextMeshProUGUI slotKeybindText, Image cooldownImage, TextMeshProUGUI cooldownText, string keyName)
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

				if (cooldownText != null)
				{
					cooldownText.text = "";
					cooldownText.enabled = false;
				}

				// Add to our tracking list so Update() knows to animate it
				_activeSkillSlots.Add(new TrackedSkillUI
				{
					PlayerSkillState = spell,
					CooldownImage = cooldownImage,
					CooldownText = cooldownText
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
		public TextMeshProUGUI CooldownText;
		public PlayerSkillStateBase PlayerSkillState;
	}
}
