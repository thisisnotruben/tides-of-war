namespace Game.Actor.Stat
{
	public class Agility : CharacterStat
	{
		public Agility(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (1.0f + manager.level) * manager.multiplier;
		}
	}
}