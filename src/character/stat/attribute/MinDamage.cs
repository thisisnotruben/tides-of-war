namespace Game.Actor.Stat
{
	public class MinDamage : CharacterStat
	{
		public MinDamage(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return manager.maxDamage.value / 2.0f;
		}
	}
}