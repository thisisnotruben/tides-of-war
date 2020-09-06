using Game.ItemPoto;
using Game.Actor;
namespace Game.Ability
{
	public class SpellProto : Commodity
	{
		/* 
					!──TODO──!
		 *	─01 []`Arcane bolt` implementation
		 *	─02 []`Bash`: ability to stun
		 *	─03 []`Divine Heal`: heals in % of health not abs
		 *	─04 []`Explosive Arrow` && `Explosive Trap`: need a blast area upon impact
		 *	─05 []`Intimidating Shout`: need a radius in which in affect
		 *	─06 []`Meteor`: need a radius in which in affect
		 *	─07 []`Overpower`: need a way to change attack table
		 *	─08 []`Piercing Shot`: need a way to ignore armor
		 *	─09 []`Siphon Mana`: need to route mana from to target -> caster
		 *	─10 []`Stomp`: need ability to stun and and area to affect
		 *	─11 []`Volley`: Is it's own special spell

				!──THOUGHTS──!
		 ?	There are a lot of similarties in the needs of these
		 ?	spells which is an area to effect on, make it an
		 ?	attribute on database (attribute would be the extants
		 ?	[radius] of the area to effect on).

		 ?	Stun would be the FSM of the character that you can
		 ?	switch to and is called here (Bash/Stomp).

		*/
		public SpellProto(Character character, string worldName) : base(character, worldName) { }
	}
}