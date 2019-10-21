namespace Game.Spell
{
    public class PiercingshotEffect : SearingarrowEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.PIERCING_SHOT);
        }
    }
}