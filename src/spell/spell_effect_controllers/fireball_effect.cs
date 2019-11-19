using Godot;
namespace Game.Ability
{
    public class FireballEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            Node2D bolt = GetNode<Node2D>("idle/bolt");
            tween.InterpolateProperty(bolt, ":modulate", bolt.GetModulate(),
                new Color(1.0f, 1.0f, 1.0f, 0.0f), lightFadeDelay,
                Tween.TransitionType.Linear, Tween.EaseType.InOut);
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            base._OnTimerTimeout();
            QueueFree();
        }
    }
}