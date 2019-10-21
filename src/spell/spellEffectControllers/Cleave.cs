namespace Game.Spell
{
    public class CleaveEffect : HasteEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.CLEAVE);
        }
    }
}