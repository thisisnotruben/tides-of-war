using Godot;
namespace Game.Ability
{
    public class haste : Spell
    {
        private float amount;
        public override float Cast()
        {
            amount = caster.animSpeed * 0.5f;
            caster.animSpeed += amount;
            SetTime(30.0f);
            caster.SetSpell(this,
                (loaded) ?
                duration - GetNode<Timer>("timer").TimeLeft :
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