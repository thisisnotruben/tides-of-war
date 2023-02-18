using Game.Actor;
using Game.GameItem;
namespace Game.Ability
{
	public class Overpower : Spell
	{
		public override Commodity Init(Character character, string worldName)
		{
			base.Init(character, worldName);

			Stats.AttackTableNode attackTable;
			attackTable.hit = 90;
			attackTable.critical = 100;
			attackTable.dodge = 100;
			attackTable.parry = 100;
			attackTable.miss = 100;
			this.attackTable = attackTable;

			return this;
		}
	}
}