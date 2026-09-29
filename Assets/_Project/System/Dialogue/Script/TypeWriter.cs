using System.Collections;
using TMPro;
using UnityEngine;
public class TypeWriter : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float typingSpeed = 0.01f;
	private string _fullText;

	private TMP_Text _textComponent;
	private Coroutine _typingCoroutine;

	public bool IsTyping { get; private set; }

	private void Awake() => _textComponent = GetComponent<TMP_Text>();

	public void StartTyping(string body)
	{
		_fullText = body;

		if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
		_typingCoroutine = StartCoroutine(TypeText(_fullText));
	}

	private IEnumerator TypeText(string text)
	{
		IsTyping = true;

		// 1. Set the text and force TMP to generate the mesh geometry
		_textComponent.text = text;
		_textComponent.ForceMeshUpdate();

		TMP_TextInfo textInfo = _textComponent.textInfo;
		int totalCharacters = textInfo.characterCount;

		// 2. Make all characters transparent initially
		for (int i = 0; i < totalCharacters; i++)
		{
			SetCharacterAlpha(i, 0);
		}

		// 3. Reveal characters one by one by setting their alpha to 255 (fully opaque)
		for (int i = 0; i < totalCharacters; i++)
		{
			SetCharacterAlpha(i, 255);
			yield return new WaitForSeconds(typingSpeed);
		}

		IsTyping = false;
	}

	private void SetCharacterAlpha(int charIndex, byte alpha)
	{
		TMP_TextInfo textInfo = _textComponent.textInfo;
		TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];

		// Skip spaces and line breaks (they have no visible geometry and will cause errors)
		if (!charInfo.isVisible) return;

		// Get the material and vertex indices for this specific character
		int meshIndex = charInfo.materialReferenceIndex;
		int vertexIndex = charInfo.vertexIndex;

		// Get the vertex colors for the mesh that this character belongs to
		Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

		// Update the alpha for all 4 vertices of the character
		vertexColors[vertexIndex + 0].a = alpha;
		vertexColors[vertexIndex + 1].a = alpha;
		vertexColors[vertexIndex + 2].a = alpha;
		vertexColors[vertexIndex + 3].a = alpha;

		// Tell TMP to update the visual mesh with the new colors
		_textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
	}

	public void FinishInstantly()
	{
		if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

		_textComponent.text = _fullText;
		_textComponent.ForceMeshUpdate();

		// Reveal all characters immediately
		TMP_TextInfo textInfo = _textComponent.textInfo;
		int totalCharacters = textInfo.characterCount;

		for (int i = 0; i < totalCharacters; i++)
		{
			SetCharacterAlpha(i, 255);
		}

		IsTyping = false;
	}
}
