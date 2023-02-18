namespace Game.Actor.Stat
{
	public class RegenTime : CharacterStat
	{
		public RegenTime(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return 60.0f - 60.0f * manager.agility.value * 0.01f;
		}
	}
}