using UnityEngine;
using TMPro;
using System.Collections;

public class DungeonUI : MonoBehaviour
{
    [Header("References")]
    private DungeonController _dungeonController;
    private DungeonEnemyTracker _dungeonEnemyTracker;
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;
    
    [Header("Announcement")]
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private CanvasGroup announcementCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    private void Awake()
    {
        _dungeonController = GetComponent<DungeonController>();
        _dungeonEnemyTracker = GetComponent<DungeonEnemyTracker>();
        
        // Hide UI by default
        if (roundText != null) roundText.gameObject.SetActive(false);
        if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(false);
        if (announcementText != null) announcementText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // We subscribe to the controller and tracker to know when to update
        if (_dungeonController != null)
        {
            _dungeonController.OnDungeonStarted += HandleDungeonStarted;
            _dungeonController.OnDungeonEnded += HandleDungeonEnded;
            _dungeonController.OnRoundStarted += HandleRoundStarted;
        }

        if (_dungeonEnemyTracker != null)
        {
            _dungeonEnemyTracker.OnEnemyCountChanged += UpdateEnemyCount;
        }
    }

    private void OnDisable()
    {
        if (_dungeonController != null)
        {
            _dungeonController.OnDungeonStarted -= HandleDungeonStarted;
            _dungeonController.OnDungeonEnded -= HandleDungeonEnded;
            _dungeonController.OnRoundStarted -= HandleRoundStarted;
        }

        if (_dungeonEnemyTracker != null)
        {
            _dungeonEnemyTracker.OnEnemyCountChanged -= UpdateEnemyCount;
        }
    }

    private void HandleDungeonStarted()
    {
        // Enable UI elements
        if (roundText != null) roundText.gameObject.SetActive(true);
        if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(true);
        if (announcementText != null) announcementText.gameObject.SetActive(true);
    }

    private void HandleDungeonEnded()
    {
        // Disable UI elements
        if (roundText != null) roundText.gameObject.SetActive(false);
        if (enemiesRemainingText != null) enemiesRemainingText.gameObject.SetActive(false);
        if (announcementText != null) announcementText.gameObject.SetActive(false);
    }
    
    private void HandleRoundStarted(int roundNumber)
    {
        // Update round text
        roundText.text = $"Round: {roundNumber}";
        // Show announcement
        StartCoroutine(ShowAnnouncement($"Round {roundNumber} Started!"));
    }
    
    private void UpdateEnemyCount(int enemyCount)
    {
        enemiesRemainingText.text = $"Enemies Remaining: {enemyCount}";
    }

    private IEnumerator ShowAnnouncement(string message)
    {
        // Update announcement text
        announcementText.text = message;
        
        // Fade in
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            announcementCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        // Fade out
        elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            announcementCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            yield return null;
        }
        
        // Hide announcement
        announcementText.text = "";
        announcementCanvasGroup.alpha = 0;
    }
}
