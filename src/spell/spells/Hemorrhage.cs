using Game.Actor;
using Godot;
namespace Game.Spell
{
    public class Hemorrhage : Spell
    {
        private Character target;
        public override float Cast()
        {
            target = caster.GetTarget();
            SetCount(5);
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
    }
}