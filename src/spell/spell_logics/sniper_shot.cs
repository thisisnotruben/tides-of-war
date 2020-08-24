using System;
using Game.Actor;
namespace Game.Ability
{
	public class sniper_shot : Spell
	{
		int amount;
		public override float Cast()
		{
			caster.weaponRange -= amount;
			return base.Cast();
		}
		public override void ConfigureSpell()
		{
			caster.SetCurrentSpell(this);
			caster.SetState(Character.States.IDLE);
			amount = (int)Math.Round(caster.weaponRange * 0.25f);
			caster.weaponRange += amount;
		}
	}
}