using System;
using Godot;
namespace Game.Ability
{
	public class fortify : Spell
	{
		private int amount;
		public override float Cast()
		{
			amount = (int)Math.Round((float)caster.armor * 0.25);
			caster.armor += amount;
			SetTime(60.0f);
			caster.SetSpell(this,
				(loaded) ? duration - GetNode<Timer>("timer").WaitTime : 0.0f);
			return base.Cast();
		}
		public override void _OnTimerTimeout()
		{
			caster.armor -= amount;
			UnMake();
		}
	}
}