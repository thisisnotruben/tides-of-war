using Godot;
using Game.Actor;
using System;

namespace Game.Spell
{
    public class Frostbolt : Spell
    {
        private Tuple<Character, float, float> values;

        public override float Cast()
        {
            values = new Tuple<Character, float, float>(
                caster.GetTarget(),
                caster.animSpeed * 0.3f,
                caster.weaponSpeed * 0.3f
            );
            values.Item1.animSpeed -= values.Item2;
            values.Item1.weaponSpeed -= values.Item3;
            SetTime(10.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            values.Item1.animSpeed += values.Item2;
            values.Item1.weaponSpeed += values.Item3;
            UnMake();
        }
        public override void ConfigureSnd()
        {
            Globals.PlaySound("frost_bolt_cast", this, new AudioStreamPlayer2D());
        }
    }
}