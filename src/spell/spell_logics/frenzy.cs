using System;
using Godot;
namespace Game.Ability
{
    public class frenzy : Spell
    {
        private Tuple<float, float> amounts;
        public override float Cast()
        {
            amounts = new Tuple<float, float>(
                caster.animSpeed * 0.25f,
                caster.weaponSpeed * 0.25f
            );
            caster.animSpeed += amounts.Item1;
            caster.weaponSpeed += amounts.Item2;
            count = 5;
            SetTime(6.0f);
            caster.SetSpell(this,
                (loaded) ? duration - GetNode<Timer>("timer").WaitTime : 0.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            caster.TakeDamage(5, false, caster, Game.Actor.Doodads.CombatText.TextType.HIT);
            count--;
            if (count > 0)
            {
                GetNode<Timer>("timer").Start();
            }
            else
            {
                caster.animSpeed -= amounts.Item1;
                caster.weaponSpeed -= amounts.Item2;
                UnMake();
            }
        }
    }
}