namespace Game.Spell
{
    public class BashEffect : FortifyEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.BASH);
        }
    }
}