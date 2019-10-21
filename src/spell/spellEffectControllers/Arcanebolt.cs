namespace Game.Spell
{
    public class ArcaneboltEffect : FireballEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.ARCANE_BOLT);
        }
    }
}