using Godot;
namespace Game.Ability
{
    public class intimidating_shout_effect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            Position = character.GetNode<Node2D>("head").Position + new Vector2(0.0f, 6.0f);
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            FadeLight(true);
            foreach (Particles2D particles2D in GetNode("idle").GetChildren())
            {
                particles2D.Emitting = false;
            }
            timer.Start();
        }
    }
}