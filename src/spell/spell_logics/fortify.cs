using System;
using Godot;
namespace Game.Ability
{
    public class fortify : Spell
    {
        private short amount;
        public override float Cast()
        {
            amount = (short)Math.Round((float)caster.armor * 0.25);
            caster.armor += amount;
            SetTime(60.0f);
            caster.SetSpell(this,
                (loaded) ? GetDuration() - GetNode<Timer>("timer").WaitTime : 0.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            caster.armor -= amount;
            UnMake();
        }
    }
}