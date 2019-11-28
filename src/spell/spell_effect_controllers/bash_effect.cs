using Godot;
namespace Game.Ability
{
    public class bash_effect : SpellEffect
    {
        public override void Init(Actor.Character character)
        {
            base.Init(character);
            fadeLight = false;
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetPosition(character.GetNode<Node2D>("head").GetPosition());
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            base._OnTimerTimeout();
            FadeLight(true);
            tween.InterpolateProperty(this, ":modulate", GetModulate(),
                new Color(1.0f, 1.0f, 1.0f, 0.0f), lightFadeDelay,
                Tween.TransitionType.Linear, Tween.EaseType.InOut);
            tween.Start();
            foreach (Particles2D particles2D in GetNode("idle").GetChildren())
            {
                particles2D.SetEmitting(false);
            }
            timer.Start();
        }
    }
}