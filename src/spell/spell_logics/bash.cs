namespace Game.Ability
{
    public class bash : Spell
    {
        public override float Cast()
        {
            target = caster.target;
            StunUnit(target, true);
            SetTime(5.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            StunUnit(target, false);
            UnMake();
        }
    }
}