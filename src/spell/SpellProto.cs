using Game.Actor;
using Game.ItemPoto;
namespace Game.Ability
{
	public class SpellProto : Commodity
	{
		/* 
					!──TODO──!
		 *	─01 [✓]`Arcane bolt` implementation
		 *	─02 [✓]`Bash`: ability to stun
		 *	─03 []`Divine Heal`: heals in % of health not abs
		 *	─04 [✓]`Explosive Arrow` && `Explosive Trap>`: need a blast area upon impact: AF
		 *	─05 [✓]`Intimidating Shout`: need a radius in which in affect: AF
		 *	─06 []`Meteor`: need a radius in which in affect: AF
		 *	─07 [✓]`Overpower`: need a way to change attack table
		 *	─08 [✓]`Piercing Shot`: need a way to ignore armor
		 *	─09 []`Siphon Mana`: need to route mana from to target -> caster
		 *	─10 [✓]`Stomp`: need ability to stun and and area to affect: AF
		 *	─11 []`Volley`: Is it's own special spell
		*/
		public Stats.AttackTableNode? attackTable = null;
		public bool ignoreArmor = false;

		public SpellProto(Character character, string worldName) : base(character, worldName) { }
	}
}