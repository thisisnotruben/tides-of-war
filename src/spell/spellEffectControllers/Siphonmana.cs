namespace Game.Spell
{
    public class SiphonmanaEffect : FireballEffect
    {
        private protected override void OnReady()
        {
            SetWorldType(WorldTypes.SIPHON_MANA);
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetGlobalPosition(character.GetTarget().GetCenterPos());
        }
    }
}