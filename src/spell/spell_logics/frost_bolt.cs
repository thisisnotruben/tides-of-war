using System;
using Game.Actor;
namespace Game.Ability
{
	public class frost_bolt : Spell
	{
		private Tuple<Character, float, float> values;
		public override float Cast()
		{
			values = new Tuple<Character, float, float>(
				caster.target,
				caster.stats.animSpeed.value * 0.3f,
				caster.stats.weaponSpeed.value * 0.3f
			);
			// values.Item1.animSpeed -= values.Item2;TODO
			// values.Item1.weaponSpeed -= values.Item3;TODO
			SetTime(10.0f);
			return base.Cast();
		}
		public override void _OnTimerTimeout()
		{
			// values.Item1.animSpeed += values.Item2;TODO
			// values.Item1.weaponSpeed += values.Item3;TODO
			UnMake();
		}
		public override void ConfigureSnd()
		{
			Globals.PlaySound("frost_bolt_cast", this, snd);
		}
	}
}