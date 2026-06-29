using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

/// <summary>
/// Reusable searchable AdvancedDropdown for a flat list of strings.
/// </summary>
public class StringSearchDropdown : AdvancedDropdown
{
	private readonly string _title;
	private readonly List<string> _items;
	private readonly Action<string> _onSelected;

	public StringSearchDropdown(AdvancedDropdownState state, string title, List<string> items, Action<string> onSelected)
		: base(state)
	{
		_title = title;
		_items = items;
		_onSelected = onSelected;
		minimumSize = new UnityEngine.Vector2(250, 300);
	}

	protected override AdvancedDropdownItem BuildRoot()
	{
		var root = new AdvancedDropdownItem(_title);
		foreach (string item in _items)
			root.AddChild(new AdvancedDropdownItem(item));
		return root;
	}

	protected override void ItemSelected(AdvancedDropdownItem item)
	{
		_onSelected?.Invoke(item.name);
	}
}
