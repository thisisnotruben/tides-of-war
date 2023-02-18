namespace Game.Actor.Stat
{
	public class HpMax : CharacterStat
	{
		public HpMax(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return (6.0f * manager.level + 24.0f + manager.stamina.value) * manager.multiplier;
		}
	}
}