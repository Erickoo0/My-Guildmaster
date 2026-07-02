using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerStatsManager : MonoBehaviour
{

	[Header("UI References")]
	[SerializeField]
	private GameObject playerStatsPanel;
	[SerializeField] private TextMeshProUGUI playerHpText;
	[SerializeField] private TextMeshProUGUI playerMpText;
	[SerializeField] private TextMeshProUGUI playerArmorText;
	[SerializeField] private TextMeshProUGUI playerLvlText;
	[SerializeField] private TextMeshProUGUI playerExpText;
	private EntityStats _entityStatsComponent;
	private Health _healthComponent;
	private Level _levelComponent;
	private Mana _manaComponent;

	[Header("SkillControllerPlayer Stats")]
	private GameObject _player;
	public static PlayerStatsManager Instance { get; private set; }

	public Health HealthComponent => _healthComponent;
	public Mana ManaComponent => _manaComponent;
	public Level LevelComponent => _levelComponent;
	public EntityStats EntityStatsComponent => _entityStatsComponent;

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
		_healthComponent = GetComponent<Health>();
		_manaComponent = GetComponent<Mana>();
		_levelComponent = GetComponent<Level>();
		_entityStatsComponent = GetComponent<EntityStats>();

		UpdateStatsMenu();
	}

	private void Start() => HandleLevelUp();

	private void Update()
	{
		if (_player != null)
			transform.position = _player.transform.position;
	}

	private void OnEnable()
	{
		// Catch and discard the broadcasted values as we dont need them here
		_healthComponent.OnHpUpdated += UpdateStatsMenu;
		_manaComponent.OnMpUpdated += UpdateStatsMenu;
		_levelComponent.OnLevelUpdated += UpdateStatsMenu;
		_levelComponent.OnLevelUpdated += HandleLevelUp;
		_levelComponent.OnExperienceGained += UpdateStatsMenu;

		EventBus.OnEntityDeathRequested += HandleEntityDeath;
	}

	private void OnDisable()
	{
		_healthComponent.OnHpUpdated -= UpdateStatsMenu;
		_manaComponent.OnMpUpdated -= UpdateStatsMenu;
		_levelComponent.OnLevelUpdated -= UpdateStatsMenu;
		_levelComponent.OnLevelUpdated -= HandleLevelUp;
		_levelComponent.OnExperienceGained -= UpdateStatsMenu;
		EventBus.OnEntityDeathRequested -= HandleEntityDeath;
	}

	private void HandleLevelUp()
	{
		if (_levelComponent == null) return;

		int currentLevel = _levelComponent.LvlCurrent;

		if (_healthComponent != null) _healthComponent.RecalculateMaxHp(currentLevel);
		if (_manaComponent != null) _manaComponent.RecalculateMaxMp(currentLevel);
	}

	private void HandleEntityDeath(GameObject entity)
	{
		// Get the level component of the dead entity and add experience to the player level component
		if (entity.TryGetComponent(out Level entityLevelComponent))
		{
			_levelComponent.AddExperience(entityLevelComponent.ExpYield);
		}
	}

	public void ToggleMenu(InputAction.CallbackContext context)
	{
		if (!context.performed) return;
		if (!playerStatsPanel.activeSelf) EventBus.RequestOpenMenu(playerStatsPanel);
		else if (playerStatsPanel.activeSelf) EventBus.RequestCloseMenu(playerStatsPanel);
	}

	private void UpdateStatsMenu()
	{
		playerHpText.text = ($"HP: {_healthComponent.HpCurrent}/{_healthComponent.HpMax}");
		playerMpText.text = ($"MP: {_manaComponent.MpCurrent}/{_manaComponent.MpMax}");
		playerLvlText.text = ($"Lvl: {_levelComponent.LvlCurrent}");
		playerExpText.text = ($"Exp: {_levelComponent.ExpCurrent}/{_levelComponent.ExpToNextLvl}");
	}
}
