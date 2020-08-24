using Godot;
namespace Game.Ability
{
	public class arcane_bolt : Spell
	{
		public override void ConfigureSnd()
		{
			Globals.PlaySound("arcane_bolt_cast", this, snd);
		}
	}
}