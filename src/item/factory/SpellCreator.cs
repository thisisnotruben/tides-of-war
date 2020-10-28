using Game.Ability;
using Game.Actor;
using Game.Database;
namespace Game.ItemPoto
{
	public class SpellCreator : CommodityCreator
	{
		protected override Commodity CreateCommodity(Character character, string worldName)
		{
			switch (worldName)
			{
				case WorldNameDB.ARCANE_BOLT:
					return new ArcaneBolt(character, worldName);
				case WorldNameDB.BASH:
					return new Bash(character, worldName);
				case WorldNameDB.EXPLOSIVE_TRAP:
					return new ExplosiveTrap(character, worldName);
				case WorldNameDB.METEOR:
					return new Meteor(character, worldName);
				case WorldNameDB.SIPHON_MANA:
					return new SiphonMana(character, worldName);
				case WorldNameDB.STOMP:
					return new Stomp(character, worldName);
				case WorldNameDB.OVERPOWER:
					return new Overpower(character, worldName);
				case WorldNameDB.VOLLEY:
					return new Volley(character, worldName);
			}

			if (SpellDB.HasSpell(worldName))
			{
				return AreaEffectDB.HasAreaEffect(worldName)
					? new SpellAreaEffect(character, worldName)
					: new SpellProto(character, worldName);
			}
			return null;
		}
	}
}