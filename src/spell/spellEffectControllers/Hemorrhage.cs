namespace Game.Spell
{
    public class HemorrhageEffect : HasteEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.HEMORRHAGE);
        }
    }
}