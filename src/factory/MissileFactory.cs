using Game.Database;
using Game.Actor;
using Game.Projectile;
namespace Game.Factory
{
	public class MissileFactory : Factory<Missile>
	{
		protected override Missile Create(Character character, string spellName)
		{
			Missile missile;

			if (MissileSpellDB.Instance.HasData(spellName))
			{
				missile = spellName switch
				{
					NameDB.Spell.METEOR => (MissileSpellOrbital)SceneDB.missileSpelOrbital.Instance(),
					_ => (MissileSpell)SceneDB.missileSpell.Instance()
				};
				((MissileSpell)missile).Init(character, character.target, spellName);
			}
			else
			{
				missile = (Missile)SceneDB.missile.Instance();
				missile.Init(character, character.target);
			}
			return missile;
		}
	}
}