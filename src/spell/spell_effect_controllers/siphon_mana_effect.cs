namespace Game.Ability
{
    public class SiphonManaEffect : FireballEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            SetGlobalPosition(character.GetTarget().GetCenterPos());
        }
    }
}