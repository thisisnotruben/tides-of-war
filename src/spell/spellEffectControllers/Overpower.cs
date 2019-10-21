namespace Game.Spell
{
    public class OverpowerEffect : HasteEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.OVERPOWER);
        }
    }
}