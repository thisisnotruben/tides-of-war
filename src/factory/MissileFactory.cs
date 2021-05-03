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
					NameDB.Spell.METEOR => SceneDB.missileSpelOrbital.Instance<MissileSpellOrbital>(),
					_ => SceneDB.missileSpell.Instance<MissileSpell>()
				}).Init(character, target, spellName)
				: SceneDB.missile.Instance<Missile>().Init(character, target);
		}
		protected override Missile Create(Character character, string worldName)
		{
			return Create(character, character.target, worldName);
		}
	}
}