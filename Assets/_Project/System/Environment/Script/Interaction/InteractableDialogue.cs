using UnityEngine;
/// <summary>
/// A interactable component for props to display dialogue.
/// </summary>
public class InteractableDialogue : MonoBehaviour, IInteractable, ISpeaker
{
	[SerializeField] private bool _isInteractable = true;

	[Header("Dialogue Data")]
	[SerializeField] private string _dialogueName;

	[Header("Use either strings or dialogue group")]
	[SerializeField] private string[] _dialogueLines;
	[SerializeField] private DialogueGroup _dialogueGroup;
	private Sprite _dialoguePortrait;
	private DialogueGroup _runTimeDialogueGroup;

	private void Awake()
	{
		// Set the potrait
		_dialoguePortrait = GetComponent<SpriteRenderer>().sprite;
	}

	public bool CanInteract() => _isInteractable && (_dialogueGroup != null || _dialogueLines.Length > 0);

	public void Interact(ControllerPlayer controllerPlayer)
	{
		if (!CanInteract())
			return;

		DialogueManager.Instance.StartDialogue(this, controllerPlayer);
	}

	public string DialogueName => _dialogueName;
	public Sprite DialoguePortrait => _dialoguePortrait;
	public DialogueGroup CurrentDialogueGroup
	{
		get
		{
			if (_dialogueGroup != null)
				return _dialogueGroup;

			if (_runTimeDialogueGroup != null)
				return _runTimeDialogueGroup;

			GenerateRuntimeDialogueGroup();
			return _runTimeDialogueGroup;
		}
	}

	public void SetInteractable(bool value) => _isInteractable = value;

	private void GenerateRuntimeDialogueGroup()
	{
		// 1. Create a blank DialogueGroup SO in memory
		_runTimeDialogueGroup = ScriptableObject.CreateInstance<DialogueGroup>();

		// 2. Create a standard DialogueNode for the text
		DialogueNode tempNode = new DialogueNode()
		{
			nodeID = "Intro",
			dialogueLines = _dialogueLines,

			// proviide some default values
			dialogueOptions = new DialogueOption[0],
			isImportant = false,
			selectionWeight = 10f
		};

		// 3. Inject the new node into the blank DialogueGroup
		_runTimeDialogueGroup.DialogueNodes.Add(tempNode);
	}
}
