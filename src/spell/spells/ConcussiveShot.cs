using System;
using Game.Actor;
namespace Game.Spell
{
    public class ConcussiveShot : Spell
    {
        private Tuple<Character, float, float> values;
        public override float Cast()
        {
            values = new Tuple<Character, float, float>(
                caster.GetTarget(),
                caster.animSpeed * 0.5f,
                caster.weaponSpeed * 0.5f
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
    }
}