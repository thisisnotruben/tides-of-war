using Godot;

namespace Game.Spell
{
    public class IntimidatingshoutEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetPosition(character.GetNode<Node2D>("head").GetPosition() + new Vector2(0.0f, 6.0f));
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            FadeLight(true);
            foreach (Particles2D particles2D in GetNode("idle").GetChildren())
            {
                particles2D.SetEmitting(false);
            }
            timer.Start();
        }
    }
}