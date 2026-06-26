using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Spell_ModifierCollection_", menuName = "Spell Modifiers/Modifier Collection")]
public class ModifierSpellCollection : ScriptableObject
{
	[SerializeField] private List<ModifierSkillBase> modifiers = new List<ModifierSkillBase>();
	
	public IReadOnlyList<ModifierSkillBase> Modifiers => modifiers;

	public void ApplyAllModifiers(SkillDataInstance skillDataInstance)
	{
		if (skillDataInstance == null)
		{
			Debug.LogWarning($"{name}: Cannot apply modifiers because spellInstance is null.");
			return;
		}
		
		foreach (ModifierSkillBase modifier in modifiers)
			if (modifier != null)
				modifier.ApplyModifier(skillDataInstance);
	}
}
