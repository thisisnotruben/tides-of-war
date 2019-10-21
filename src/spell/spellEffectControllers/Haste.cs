using Godot;

namespace Game.Spell
{
    public class HasteEffect : SpellEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.HASTE);
        }
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