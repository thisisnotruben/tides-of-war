namespace Game.Spell
{
    public class SnipershotEffect : ExplosivearrowEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.SNIPER_SHOT);
        }
    }
}