namespace Game.Spell
{
    public class FrostboltEffect : FireballEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.FROST_BOLT);
        }
    }
}