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
				NameDB.Spell.ARCANE_BOLT => SceneDB.arcaneBoltAreaEffect.Instance<ArcaneBolt>(),
				NameDB.Spell.BASH => new Bash(),
				NameDB.Spell.EXPLOSIVE_TRAP => new ExplosiveTrap(),
				NameDB.Spell.OVERPOWER => new Overpower(),
				NameDB.Spell.SIPHON_MANA => new SiphonMana(),
				NameDB.Spell.STOMP => SceneDB.stompAreaEffect.Instance<Stomp>(),
				NameDB.Spell.VOLLEY => new Volley(),
				_ => Globals.spellDB.HasData(worldName)
					? Globals.areaEffectDB.HasData(worldName)
						? SceneDB.spellAreaEffect.Instance<SpellAreaEffect>()
						: new Spell()
					: null
			};

			spell?.Init(character, worldName);
			return spell;
		}
	}
}