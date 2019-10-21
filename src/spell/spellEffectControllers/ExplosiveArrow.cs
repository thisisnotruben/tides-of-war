using Godot;

namespace Game.Spell
{
    public class ExplosivearrowEffect : SpellEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.EXPLOSIVE_ARROW);
        }
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