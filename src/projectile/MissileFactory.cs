using Game.Database;
using Game.Actor;
using Game.Ability;
namespace Game.Projectile
{
	public static class MissileFactory
	{
		public static Missile CreateMissile(Character character, SpellProto spell = null)
		{
			Missile missile;

			if (spell != null)
			{
				missile = (MissileSpell)MissileSpell.scene.Instance();
				((MissileSpell)missile).Init(character, character.target, spell.worldName);
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