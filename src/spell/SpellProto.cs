using Game.Actor;
using Game.ItemPoto;
namespace Game.Ability
{
	public class SpellProto : Commodity
	{
		/* 
					!──TODO──!
		 *	─01 []`Meteor`: need a radius in which in affect: AF
		 *	─02 []`Volley`: Is it's own special spell
		*/
		public Stats.AttackTableNode? attackTable = null;
		public bool ignoreArmor = false;

		public SpellProto(Character character, string worldName) : base(character, worldName) { }
	}
}