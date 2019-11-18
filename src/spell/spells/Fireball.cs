using Godot;
using Game.Actor;

namespace Game.Spell
{
    public class Fireball : Spell
    {
        private Character target;

        public override float Cast()
        {
            target = caster.GetTarget();
            SetCount(2);
            SetTime(15.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            target.TakeDamage(5, false, caster, Game.Misc.Other.CombatText.TextType.HIT);
            count--;
            if (count > 0)
            {
                GetNode<Timer>("timer").Start();
            }
            else
            {
                UnMake();
            }
        }
        public override void ConfigureSnd()
        {
            Globals.PlaySound("fireball_cast", this, new AudioStreamPlayer2D());
        }
    }
}