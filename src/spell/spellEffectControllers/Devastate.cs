namespace Game.Spell
{
    public class DevastateEffect : HasteEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.DEVASTATE);
        }
    }
}