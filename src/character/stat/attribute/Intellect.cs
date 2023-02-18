namespace Game.Actor.Stat
{
	public class Intellect : CharacterStat
	{
		public Intellect(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (2.0f + manager.level) * manager.multiplier;
		}
	}
}