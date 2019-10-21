namespace Game.Spell
{
    public class MeteorEffect : FireballEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.METEOR);
            SetProcess(true);

        }
    }
}