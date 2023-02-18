namespace Game.Actor.Stat
{
	public class Stamina : CharacterStat
	{
		public Stamina(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (3.0f + manager.level) * manager.multiplier;
		}
	}
}