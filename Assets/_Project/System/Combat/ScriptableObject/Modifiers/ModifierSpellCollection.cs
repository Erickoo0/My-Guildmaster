using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Spell_ModifierCollection_", menuName = "Spell Modifiers/Modifier Collection")]
public class ModifierSpellCollection : ScriptableObject
{
	[SerializeField] private List<ModifierBaseSpell> modifiers = new List<ModifierBaseSpell>();
	
	public IReadOnlyList<ModifierBaseSpell> Modifiers => modifiers;

	public void ApplyAllModifiers(SpellDataInstance spellDataInstance)
	{
		if (spellDataInstance == null)
		{
			Debug.LogWarning($"{name}: Cannot apply modifiers because spellInstance is null.");
			return;
		}
		
		foreach (ModifierBaseSpell modifier in modifiers)
			if (modifier != null)
				modifier.ApplyModifier(spellDataInstance);
	}
}
