using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class QuestUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private GameObject questMenuPanel;
    [SerializeField] private GameObject questSoMenuPanel;
    [SerializeField] private GameObject questMenuPrefab;
    [SerializeField] private Image questPanel;
    [SerializeField] private GameObject questSoPanel;
    [SerializeField] private GameObject questPrefab;

    private List<GameObject> _spawnedUIElements = new List<GameObject>();

    private void OnEnable() => EventBus.OnUpdateQuestRequested += RefreshUI;
    private void OnDisable() => EventBus.OnUpdateQuestRequested -= RefreshUI;

    private void RefreshUI()
    {
        // 1. Wipe out the old visual elements
        foreach (GameObject uiElement in _spawnedUIElements)
        {
            Destroy(uiElement);
        }
        _spawnedUIElements.Clear();

        // 2. Safely read directly from the Manager's updated data
        if (QuestManager.Instance == null) return;

        foreach (QuestActive activeQuest in QuestManager.Instance.QuestList)
        {
            GameObject newQuestVisual = Instantiate(questPrefab, questSoPanel.transform);
            newQuestVisual.GetComponent<Quest>().Setup(activeQuest);
            newQuestVisual.GetComponent<Quest>().UpdateProgressText(activeQuest);
            
            _spawnedUIElements.Add(newQuestVisual);
        }
    }
    
    public void ToggleMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!questMenuPanel.activeSelf)EventBus.RequestOpenMenu(questMenuPanel);
        else if (questMenuPanel.activeSelf) EventBus.RequestCloseMenu(questMenuPanel);
    }
}
