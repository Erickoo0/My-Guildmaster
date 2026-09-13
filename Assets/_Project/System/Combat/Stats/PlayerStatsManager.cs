using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerStatsManager : MonoBehaviour
{

	[Header("References")]
	[SerializeField] private SpriteRenderer _playerSpriteRenderer;
	[SerializeField] private GameObject playerStatsPanel;

	[Header("Core Stats")]
	[SerializeField] private Image playerPortrait;
	[SerializeField] private TextMeshProUGUI playerHpText;
	[SerializeField] private TextMeshProUGUI playerMpText;
	[SerializeField] private TextMeshProUGUI playerLvlText;
	[SerializeField] private TextMeshProUGUI playerExpText;

	[Header("Combat & Defense UI")]
	[SerializeField] private TextMeshProUGUI playerArmorText;
	[SerializeField] private TextMeshProUGUI playerAttackText;
	[SerializeField] private TextMeshProUGUI playerElementalStatsText;

	private GameObject _player;
	public static PlayerStatsManager Instance { get; private set; }

	public Health HealthComponent { get; private set; }
	public Mana ManaComponent { get; private set; }
	public Level LevelComponent { get; private set; }
	public EntityStats EntityStatsComponent { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			Debug.unityLogger.Log("Multiple PlayerStatsManagers detected. Disabling script.");
			return;
		}

		Instance = this;

		// Get the components
		_player = GameObject.FindGameObjectWithTag("Player");
		var visualComponent = _player.transform.Find("Visual");
		_playerSpriteRenderer = visualComponent.GetComponent<SpriteRenderer>();
		HealthComponent = GetComponent<Health>();
		ManaComponent = GetComponent<Mana>();
		LevelComponent = GetComponent<Level>();
		EntityStatsComponent = GetComponent<EntityStats>();

		UpdateStatsMenu();
	}

	private void Start()
	{
		SyncStatsToLevel();
		UpdateStatsMenu();
	}

	private void LateUpdate()
	{
		if (_player != null)
			transform.position = _player.transform.position;
	}

	private void OnEnable()
	{
		HealthComponent.OnHpUpdated += UpdateStatsMenu;
		ManaComponent.OnMpUpdated += UpdateStatsMenu;
		LevelComponent.OnLevelUpdated += UpdateStatsMenu;
		LevelComponent.OnLevelUpdated += SyncStatsToLevel;
		LevelComponent.OnExperienceGained += UpdateStatsMenu;
		EventBus.OnEntityDeathRequested += HandleEntityDeath;
	}

	private void OnDisable()
	{
		HealthComponent.OnHpUpdated -= UpdateStatsMenu;
		ManaComponent.OnMpUpdated -= UpdateStatsMenu;
		LevelComponent.OnLevelUpdated -= UpdateStatsMenu;
		LevelComponent.OnLevelUpdated -= SyncStatsToLevel;
		LevelComponent.OnExperienceGained -= UpdateStatsMenu;
		EventBus.OnEntityDeathRequested -= HandleEntityDeath;
	}

	private void SyncStatsToLevel()
	{
		int currentLvl = LevelComponent.LvlCurrent;

		HealthComponent.RecalculateMaxHp(currentLvl);
		ManaComponent.RecalculateMaxMp(currentLvl);
		EntityStatsComponent.RecalculateStats(currentLvl);
	}

	private void HandleEntityDeath(GameObject entity)
	{
		// Get the level component of the dead entity and add its xp experience to the player level component
		if (entity.TryGetComponent(out Level entityLevelComponent))
			LevelComponent.AddExperience(entityLevelComponent.ExpYield);

	}

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (!playerStatsPanel.activeSelf) EventBus.RequestOpenMenu(playerStatsPanel);
		else if (playerStatsPanel.activeSelf) EventBus.RequestCloseMenu(playerStatsPanel);

		playerPortrait.sprite = _playerSpriteRenderer.sprite;

	}

	private void UpdateStatsMenu()
	{
		if (playerHpText != null && HealthComponent != null)
			playerHpText.text = $"HP: {HealthComponent.HpCurrent}/{HealthComponent.HpMax}";

		if (playerMpText != null && ManaComponent != null)
			playerMpText.text = $"MP: {ManaComponent.MpCurrent}/{ManaComponent.MpMax}";

		if (playerLvlText != null && LevelComponent != null)
			playerLvlText.text = $"Lvl: {LevelComponent.LvlCurrent}";

		if (playerExpText != null && LevelComponent != null)
			playerExpText.text = $"Xp: {LevelComponent.ExpCurrent}/{LevelComponent.ExpToNextLvl}";

		// Combat Stats Update
		if (EntityStatsComponent != null)
		{
			if (playerArmorText != null)
				playerArmorText.text = $"Defense: {EntityStatsComponent.Defense:F0}";

			if (playerAttackText != null)
				playerAttackText.text = $"Atk Power: {EntityStatsComponent.AttackPower:F0}";

			if (playerElementalStatsText != null)
			{
				playerElementalStatsText.text =
					$"Fire Power: {EntityStatsComponent.AttackPowerFire:F0}\n" +
					$"Water Power: {EntityStatsComponent.AttackPowerWater:F0}\n" +
					$"Earth Power: {EntityStatsComponent.AttackPowerEarth:F0}\n" +
					$"Air Power: {EntityStatsComponent.AttackPowerAir:F0}\n" +
					$"Lightning Power: {EntityStatsComponent.AttackPowerLightning:F0}\n" +
					$"Holy Power: {EntityStatsComponent.AttackPowerHoly:F0}\n" +
					$"Dark Power: {EntityStatsComponent.AttackPowerDark:F0}";
			}
		}
	}
}
