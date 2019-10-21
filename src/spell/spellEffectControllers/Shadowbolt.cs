namespace Game.Spell
{
    public class ShadowboltEffect : FireballEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.SHADOW_BOLT);
        }
    }
}