using Game.Actor;
namespace Game.Ability
{
	public class Overpower : SpellProto
	{
		public Overpower(Character character, string worldName) : base(character, worldName)
		{
			Stats.AttackTableNode attackTable;
			attackTable.hit = 90;
			attackTable.critical = 100;
			attackTable.dodge = 100;
			attackTable.parry = 100;
			attackTable.miss = 100;
			this.attackTable = attackTable;
		}
	}
}