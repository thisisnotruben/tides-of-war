using Godot;
namespace Game.Ability
{
    public class fireball : Spell
    {
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