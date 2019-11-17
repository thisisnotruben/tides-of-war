namespace Game.Spell
{
    public class SiphonmanaEffect : FireballEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetGlobalPosition(character.GetTarget().GetCenterPos());
        }
    }
}