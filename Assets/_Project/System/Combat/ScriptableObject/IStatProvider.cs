using UnityEngine;

public interface IStatProvider
{
	Health EntityHealth { get; }
	Mana EntityMana { get; }
	Level EntityLevel { get; }
}
