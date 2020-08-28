using System;
using Game.Actor.State;
namespace Game.Ability
{
	public class sniper_shot : Spell
	{
		int amount;
		public override float Cast()
		{
			// caster.weaponRange -= amount;TODO
			return base.Cast();
		}
		public override void ConfigureSpell()
		{
			caster.SetCurrentSpell(this);
			caster.state = FSM.State.IDLE;
			amount = (int)Math.Round(caster.stats.weaponRange.value * 0.25f);
			// caster.weaponRange += amount;TODO
		}
	}
}