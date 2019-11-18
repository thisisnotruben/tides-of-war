using Godot;
namespace Game.Spell
{
    public class Haste : Spell
    {
        private float amount;
        public override float Cast()
        {
            amount = caster.animSpeed * 0.5f;
            caster.animSpeed += amount;
            SetTime(30.0f);
            caster.SetSpell(this,
                (loaded) ?
                GetDuration() - GetNode<Timer>("timer").GetTimeLeft() :
                0.0f
            );
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            caster.animSpeed -= amount;
            UnMake();
        }
    }
}