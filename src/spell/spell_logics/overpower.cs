namespace Game.Ability
{
	public class overpower : Spell
	{
		public override void Init(string worldName)
		{
			base.Init(worldName);
			attackTable.hit = 90;
			attackTable.critical = 100;
			attackTable.dodge = 100;
			attackTable.parry = 100;
			attackTable.miss = 100;
		}
	}
}