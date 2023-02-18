namespace Game.Actor.Stat
{
	public class MaxDamage : CharacterStat
	{
		public MaxDamage(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return manager.hpMax.value * 0.225f;
		}
	}
}