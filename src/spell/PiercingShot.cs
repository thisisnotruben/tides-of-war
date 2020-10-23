using Game.Actor;
namespace Game.Ability
{
	public class PiercingShot : SpellProto
	{
		public PiercingShot(Character character, string worldName) : base(character, worldName)
		{
			ignoreArmor = true;
		}
	}
}