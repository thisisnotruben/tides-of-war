namespace Game.Actor.Stat
{
	public class RegenAmount : CharacterStat
	{
		public RegenAmount(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (6.0f * manager.level + 24.0f + ((3.0f + manager.level) * manager.multiplier)) * manager.multiplier * 0.05f;
		}
	}
}