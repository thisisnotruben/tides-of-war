using Game.Ability;
using Game.Actor;
using Game.Database;
using Game.GameItem;
namespace Game.Factory
{
	public class SpellFactory : CommodityFactory
	{
		protected override Commodity CreateCommodity(Character character, string worldName)
		{
			Spell spell = null;

			switch (worldName)
			{
				case NameDB.Spell.ARCANE_BOLT:
					spell = (ArcaneBolt)SceneDB.arcaneBoltAreaEffect.Instance();
					break;
				case NameDB.Spell.BASH:
					spell = new Bash();
					break;
				case NameDB.Spell.EXPLOSIVE_TRAP:
					spell = new ExplosiveTrap();
					break;
				case NameDB.Spell.OVERPOWER:
					spell = new Overpower();
					break;
				case NameDB.Spell.SIPHON_MANA:
					spell = new SiphonMana();
					break;
				case NameDB.Spell.STOMP:
					spell = (Stomp)SceneDB.stompAreaEffect.Instance();
					break;
				case NameDB.Spell.VOLLEY:
					spell = new Volley();
					break;
				default:
					if (SpellDB.HasSpell(worldName))
					{
						spell = AreaEffectDB.HasAreaEffect(worldName)
							? (SpellAreaEffect)SceneDB.spellAreaEffect.Instance()
							: new Spell();
					}
					break;
			}

			spell?.Init(character, worldName);
			return spell;
		}
	}
}