using Godot;
namespace Game.Ability
{
    public class frenzy_effect : SpellEffect
    {
        public override void Init(Actor.Character character)
        {
            base.Init(character);
            fadeLight = false;
        }
        public override void OnHit(Spell spell)
        {
            base.OnHit(spell);
            Position = character.GetNode<Node2D>("head").Position;
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
                particles2D.Emitting = false;
            }
            timer.Start();
        }
    }
}