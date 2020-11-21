using Game.Ability;
using Game.Actor;
using Game.Database;
namespace Game.Factory
{
	public class SpellFactory : Factory<Spell>
	{
		protected override Spell Create(Character character, string worldName)
		{
			Spell spell = worldName switch
			{
				NameDB.Spell.ARCANE_BOLT => (ArcaneBolt)SceneDB.arcaneBoltAreaEffect.Instance(),
				NameDB.Spell.BASH => new Bash(),
				NameDB.Spell.EXPLOSIVE_TRAP => new ExplosiveTrap(),
				NameDB.Spell.OVERPOWER => new Overpower(),
				NameDB.Spell.SIPHON_MANA => new SiphonMana(),
				NameDB.Spell.STOMP => (Stomp)SceneDB.stompAreaEffect.Instance(),
				NameDB.Spell.VOLLEY => new Volley(),
				_ => SpellDB.Instance.HasData(worldName)
					? AreaEffectDB.Instance.HasData(worldName)
						? (SpellAreaEffect)SceneDB.spellAreaEffect.Instance()
						: new Spell()
					: null
			};

			spell?.Init(character, worldName);
			return spell;
		}
	}
}