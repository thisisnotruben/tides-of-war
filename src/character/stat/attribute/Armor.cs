namespace Game.Actor.Stat
{
	public class Armor : CharacterStat
	{
		public Armor(StatManager statManager) : base(statManager) { }
		public override float CalculateBaseValue()
		{
			return ((manager.stamina.value + manager.agility.value) / 2.0f)
				* ((manager.minDamage.value + manager.maxDamage.value) / 2.0f) * 0.01f;
		}
	}
}