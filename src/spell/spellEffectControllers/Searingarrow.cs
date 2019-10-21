namespace Game.Spell
{
    public class SearingarrowEffect : SpellEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.SEARING_ARROW);
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
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