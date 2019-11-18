namespace Game.Spell
{
    public class Bash : Spell
    {
        public override float Cast()
        {
            target = caster.GetTarget();
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