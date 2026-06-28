using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Orchestrates the Skill Tree UI panel.
/// Spawns SkillNodeUI instances for every node in a SkillTree, draws
/// prerequisite connection lines, and refreshes all nodes when the ledger changes.
/// </summary>
public class SkillTreeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _skillTreePanel;
    [SerializeField] private SkillTree _skillTree;
    [SerializeField] private RectTransform _skillNodeContainer;
    [SerializeField] private GameObject _skillNodeUIPrefab;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private TextMeshProUGUI _totalSkillPointsText;

    [Header("Connection Lines")]
    [Tooltip("Prefab with a UI Image used as a line segment between nodes.")]
    [SerializeField] private RectTransform _connectionLinePrefab;

    [Header("Layout")]
    [Tooltip("Multiplier applied to SkillNode.UIPosition to convert design units to pixels.")]
    [SerializeField] private float _positionScale = 100f;

    private SkillTreeLedger _skillTreeLedger;

    private readonly List<SkillNodeUI> _skillNodesUIList = new List<SkillNodeUI>();
    private readonly List<RectTransform> _connectionLinesList = new List<RectTransform>();
    private readonly Dictionary<string, Vector2> _nodePositions = new Dictionary<string, Vector2>();

    private void OnEnable()
    {
        if (_skillTree == null)
        {
            Debug.LogWarning($"{name}: No SkillTree assigned in the Inspector.");
            return;
        }

        _skillTreeLedger = SkillTreeCompiler.Instance.GetOrCreateSkillTreeLedger(_skillTree);
        Rebuild();
    }

    private void OnDisable() => ClearAll();

    public void ToggleMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!_skillTreePanel.activeSelf)
            EventBus.RequestOpenMenu(_skillTreePanel);
        else
            EventBus.RequestCloseMenu(_skillTreePanel);
    }

    private void Rebuild()
    {
        ClearAll();

        if (_skillTree == null || _skillTreeLedger == null)
        {
            Debug.LogWarning($"{name}: Cannot build SkillTreeUI — SkillTree or Ledger is null.");
            return;
        }

        if (_skillNameText != null)
            _skillNameText.text = _skillTree.SkillData != null ? _skillTree.SkillData.Name : _skillTree.name;

        // 1. Spawn a SkillNodeUI for every node in the tree
        foreach (SkillNode node in _skillTree.SkillNodes)
        {
            if (node == null) continue;
            SpawnSkillNodeUI(node);
        }

        // 2. Draw connections between prerequisite nodes
        DrawAllConnections();

        // 3. Initial visual refresh
        RefreshAllNodes();
    }

    private void SpawnSkillNodeUI(SkillNode node)
    {
        if (_skillNodeUIPrefab == null)
        {
            Debug.LogWarning($"{name}: SkillNodeUI prefab is not assigned.");
            return;
        }

        GameObject go = Instantiate(_skillNodeUIPrefab, _skillNodeContainer);
        RectTransform rt = go.GetComponent<RectTransform>();

        Vector2 nodePos = node.UIPosition * _positionScale;
        rt.anchoredPosition = nodePos;
        
        _nodePositions[node.ID] = nodePos;

        SkillNodeUI nodeUI = go.GetComponent<SkillNodeUI>();
        if (nodeUI == null)
        {
            Debug.LogWarning($"{name}: Spawned prefab is missing a SkillNodeUI component.");
            return;
        }

        nodeUI.Setup(node, _skillTree, _skillTreeLedger, RefreshAllNodes);
        _skillNodesUIList.Add(nodeUI);
    }

    private void DrawAllConnections()
    {
        if (_connectionLinePrefab == null) return;

        foreach (SkillNode node in _skillTree.SkillNodes)
        {
            if (node == null || node.Prerequisites == null) continue;

            foreach (SkillNodePrerequisite prereq in node.Prerequisites)
            {
                if (prereq == null) continue;

                if (!_nodePositions.TryGetValue(node.ID, out Vector2 toPos)) continue;
                if (!_nodePositions.TryGetValue(prereq.RequiredSkillNodeID, out Vector2 fromPos)) continue;

                DrawConnection(fromPos, toPos);
            }
        }
    }

    private void DrawConnection(Vector2 fromPos, Vector2 toPos)
    {
        RectTransform line = Instantiate(_connectionLinePrefab, _skillNodeContainer);
        line.SetAsFirstSibling(); // render behind nodes
        
        Vector2 delta = toPos - fromPos;
        line.sizeDelta        = new Vector2(delta.magnitude, line.sizeDelta.y);
        line.anchoredPosition = fromPos;
        line.localRotation    = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

        _connectionLinesList.Add(line);
    }

    private void RefreshAllNodes()
    {
        foreach (SkillNodeUI nodeUI in _skillNodesUIList)
            nodeUI.RefreshUI();

        UpdateSkillPointsText();
    }

    private void UpdateSkillPointsText()
    {
        if (_totalSkillPointsText == null || _skillTreeLedger == null) return;
        _totalSkillPointsText.text = $"Points Spent: {_skillTreeLedger.GetTotalAllocatedSkillPoints()}";
    }

    private void ClearAll()
    {
        foreach (SkillNodeUI nodeUI in _skillNodesUIList)
            if (nodeUI != null) Destroy(nodeUI.gameObject);

        foreach (RectTransform line in _connectionLinesList)
            if (line != null) Destroy(line.gameObject);

        _skillNodesUIList.Clear();
        _connectionLinesList.Clear();
        _nodePositions.Clear();
    }
}