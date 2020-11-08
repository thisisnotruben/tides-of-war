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
				case WorldNameDB.ARCANE_BOLT:
					spell = (ArcaneBolt)SceneDB.arcaneBoltAreaEffect.Instance();
					break;
				case WorldNameDB.BASH:
					spell = new Bash();
					break;
				case WorldNameDB.EXPLOSIVE_TRAP:
					spell = new ExplosiveTrap();
					break;
				case WorldNameDB.OVERPOWER:
					spell = new Overpower();
					break;
				case WorldNameDB.SIPHON_MANA:
					spell = new SiphonMana();
					break;
				case WorldNameDB.STOMP:
					spell = (Stomp)SceneDB.stompAreaEffect.Instance();
					break;
				case WorldNameDB.VOLLEY:
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