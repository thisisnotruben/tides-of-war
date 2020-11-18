using Game.Database;
using Game.Actor;
using Game.Projectile;
namespace Game.Factory
{
	public static class MissileFactory
	{
		public static Missile CreateMissile(Character character, string spellName)
		{
			Missile missile;

			if (SpellDB.HasSpellMissile(spellName))
			{
				switch (spellName)
				{
					case NameDB.Spell.METEOR:
						missile = (MissileSpellOrbital)SceneDB.missileSpelOrbital.Instance();
						break;
					default:
						missile = (MissileSpell)SceneDB.missileSpell.Instance();
						break;
				}
				((MissileSpell)missile).Init(character, character.target, spellName);
			}
			else
			{
				missile = CreateMissile(character);
			}

			return missile;
		}
		public static Missile CreateMissile(Character character)
		{
			Missile missile = (Missile)SceneDB.missile.Instance();
			missile.Init(character, character.target);
			return missile;
		}
	}
}