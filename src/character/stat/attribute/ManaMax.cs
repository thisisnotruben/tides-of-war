namespace Game.Actor.Stat
{
	public class ManaMax : CharacterStat
	{
		public ManaMax(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (6.0f * manager.level + 16.0f + manager.intellect.value) * manager.multiplier;
		}
	}
}