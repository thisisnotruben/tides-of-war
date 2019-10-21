namespace Game.Spell
{
    public class StingingshotEffect : SearingarrowEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.STINGING_SHOT);
        }
    }
}