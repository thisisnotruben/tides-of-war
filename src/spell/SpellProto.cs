using Game.Actor;
using Game.ItemPoto;
namespace Game.Ability
{
	public class SpellProto : Commodity
	{
		/* 
					!──TODO──!
		 *	─01 []`Meteor`: need a radius in which in affect: AF
		*/
		public Stats.AttackTableNode? attackTable = null;

		public SpellProto(Character character, string worldName) : base(character, worldName) { }
	}
}