namespace Game.Spell
{
    public class DivinehealEffect : FortifyEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.DIVINE_HEAL);
        }
    }
}