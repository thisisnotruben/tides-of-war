using Godot;

namespace Game.Spell
{
    public class MeteorEffect : FireballEffect
    {
        [Signal]
        public delegate void Hit(bool hit);

        public override void _Ready()
        {
            base._Ready();
            SetProcess(true);
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            EmitSignal(nameof(Hit), true);
        }
    }
}