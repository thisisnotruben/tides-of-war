using Game.Actor;
using Game.GameItem;
namespace Game.Ability
{
	public class Spell : Commodity
	{
		/* 
					!──TODO──!
		 *	─01 []`Meteor`: need a radius in which in affect: AF
		*/
		public Stats.AttackTableNode? attackTable = null;

		public Spell(Character character, string worldName) : base(character, worldName) { }
	}
}