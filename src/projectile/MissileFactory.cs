using Game.Database;
using Game.Actor;
namespace Game.Projectile
{
	public static class MissileFactory
	{
		public static Missile CreateMissile(Character character, string spellName)
		{
			Missile missile;

			if (!spellName.Equals(string.Empty) && SpellDB.HasSpellMissile(spellName))
			{
				missile = (MissileSpell)MissileSpell.scene.Instance();
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
			Missile missile = (Missile)Missile.scene.Instance();
			missile.Init(character, character.target);
			return missile;
		}
	}
}