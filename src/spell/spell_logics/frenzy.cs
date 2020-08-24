using System;
using Game.Actor.Doodads;
using Godot;
namespace Game.Ability
{
	public class frenzy : Spell
	{
		private int damage = 5;
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
			caster.Harm(damage);
			caster.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
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