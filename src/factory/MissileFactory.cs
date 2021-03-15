using Game.Database;
using Game.Actor;
using Game.Projectile;
namespace Game.Factory
{
	public class MissileFactory : Factory<Missile>
	{
		protected override Missile Create(Character character, Character target, string spellName)
		{
			return Globals.missileSpellDB.HasData(spellName)
				? (spellName switch
				{
					NameDB.Spell.METEOR => (MissileSpellOrbital)SceneDB.missileSpelOrbital.Instance(),
					_ => (MissileSpell)SceneDB.missileSpell.Instance()
				}).Init(character, target, spellName)
				: ((Missile)SceneDB.missile.Instance()).Init(character, target);
		}
		protected override Missile Create(Character character, string worldName)
		{
			return Create(character, character.target, worldName);
		}
	}
}