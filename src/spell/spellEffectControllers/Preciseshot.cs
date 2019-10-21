namespace Game.Spell
{
    public class PreciseshotEffect : SearingarrowEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.PRECISE_SHOT);
        }
    }
}