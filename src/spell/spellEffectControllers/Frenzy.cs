using Godot;

namespace Game.Spell
{
    public class FrenzyEffect : SpellEffect
    {
        public override void OnHit(Spell spell)
        {
            base.OnHit(spell);
            SetPosition(character.GetNode<Node2D>("head").GetPosition());
            character.GetParent().MoveChild(this, 0);
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            base._OnTimerTimeout();
            FadeLight(true);
            foreach (Particles2D particles2D in GetNode("idle").GetChildren())
            {
                particles2D.SetEmitting(false);
            }
            timer.Start();
        }
    }
}