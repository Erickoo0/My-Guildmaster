using TMPro;
using UnityEngine;
using System.Collections;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject speechBubblePrefab;
    private Npc _npc;
    
    [Header("Speech Bubble Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float speechBubbleMinWaitTime = 3f;
    [SerializeField] private float speechBubbleMaxWaitTime = 9f;
    [SerializeField] private float speechBubbleDuration = 2.5f;

    private float _timer;
    private GameObject _currentBubble;
    private TypeWriter _typeWriter;
    
    private void Start()
    {
        _npc = GetComponent<Npc>();
        SetRandomWaitTime();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            if (_currentBubble != null) DespawnBubble();
            else SpawnBubble();
        }
    }

    private void SpawnBubble()
    {
        // 1. Get speech bubble lines from NPC
        string[] speechBubbleLines = _npc.CurrentSpeechBubble;
        
        // Safety Check
        if (speechBubblePrefab == null || speechBubbleLines == null || speechBubbleLines.Length == 0) return;
        
        // 1. Create bubble and parent it to NPC with offset
        _currentBubble = Instantiate(speechBubblePrefab, transform.position + spawnOffset, Quaternion.identity, transform);
        
        // 2. Find the TextMeshPro component in the children and set the text
        TypeWriter typewriter = _currentBubble.GetComponentInChildren<TypeWriter>();
        if (typewriter != null) typewriter.StartTyping(speechBubbleLines[Random.Range(0, speechBubbleLines.Length)]);
        
        // 3. Set alive timer 
        _timer = speechBubbleDuration;
    }

    private void DespawnBubble()
    {
        Destroy(_currentBubble);
        _currentBubble = null;
        SetRandomWaitTime();
    }
    
    private void SetRandomWaitTime() => _timer = Random.Range(speechBubbleMinWaitTime, speechBubbleMaxWaitTime);
    
}
