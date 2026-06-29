using UnityEngine;

/// <summary>
/// Renders a string field as a searchable dropdown of all float fields
/// on the Effect type named by a sibling string field.
/// </summary>
public class EffectFieldSelectorAttribute : PropertyAttribute
{
	/// <summary>
	/// The name of the sibling serialized field that holds the Effect type name string.
	/// </summary>
	public string TypeFieldName { get; }

	public EffectFieldSelectorAttribute(string typeFieldName)
	{
		TypeFieldName = typeFieldName;
	}
}
