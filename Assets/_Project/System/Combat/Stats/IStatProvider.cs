public interface IStatProvider
{
	Health EntityHealth { get; }
	Mana EntityMana { get; }
	Level EntityLevel { get; }
	EntityStats EntityStats { get; }
}
