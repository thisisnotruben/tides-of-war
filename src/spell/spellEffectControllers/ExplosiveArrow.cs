using Godot;
namespace Game.Spell
{
    public class ExplosivearrowEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            GetNode<Node2D>("light").Show();
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