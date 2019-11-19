using Godot;
namespace Game.Ability
{
    public class Shadowbolt : Spell
    {
        public override void ConfigureSnd()
        {
            Globals.PlaySound("shadow_bolt_cast", this, new AudioStreamPlayer2D());
        }
    }
}