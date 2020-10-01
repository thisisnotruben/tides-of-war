using Game.Database;
using Game.Actor;
namespace Game.Projectile
{
	public static class MissileFactory
	{
		public static Missile CreateMissile(Character character, string missileType)
		{
			Missile missile;

			if (SpellDB.HasSpell(missileType))
			{
				missile = (MissileSpell)MissileSpell.scene.Instance();
				((MissileSpell)missile).Init(character, character.target, missileType);
			}
			else
			{
				missile = (Missile)Missile.scene.Instance();
				missile.Init(character, character.target);
			}

			return missile;
		}
	}
}