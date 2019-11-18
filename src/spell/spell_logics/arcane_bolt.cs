using Godot;
namespace Game.Spell
{
    public class ArcaneBolt : Spell
    {
        public override void ConfigureSnd()
        {
            Globals.PlaySound("arcane_bolt_cast", this, new AudioStreamPlayer2D());
        }
    }
}