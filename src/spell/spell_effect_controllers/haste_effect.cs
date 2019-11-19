using Godot;
namespace Game.Ability
{
    public class HasteEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetPosition(character.GetNode<Node2D>("img").GetPosition());
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