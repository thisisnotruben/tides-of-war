using Game.Misc.Missile;
using Godot;
namespace Game.Ability
{
    public class slow_effect : SpellEffect
    {
        public override void Init(Actor.Character character)
        {
            base.Init(character);
            fadeLight = false;
            lightFadeDelay = 4.0f;
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            Bolt bolt = GetOwner()as Bolt;
            if (bolt != null)
            {
                GetParent().RemoveChild(this);
                bolt.GetTarget().AddChild(this);
                SetPosition(bolt.GetTarget().GetNode<Node2D>("img").GetPosition());
                spell.Connect(nameof(Unmake), this, nameof(_OnTimerTimeout));
                tween.Start();
                timer.Start();
            }
            else
            {
                GD.Print("Unexpected parent in class: Slow");
            }
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