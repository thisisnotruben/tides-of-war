using Game.Actor.Doodads;
using Godot;
namespace Game.Ability
{
	public class fireball : Spell
	{
		private int damage = 5;

		public override float Cast()
		{
			target = caster.target;
			count = 2;
			SetTime(15.0f);
			return base.Cast();
		}
		public override void _OnTimerTimeout()
		{
			target.Harm(damage);
			target.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
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
			Globals.PlaySound("fireball_cast", this, snd);
		}
	}
}