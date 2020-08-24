using Game.Actor.Doodads;
using Godot;
namespace Game.Ability
{
	public class stinging_shot : Spell
	{
		private int damage = 5;

		public override float Cast()
		{
			target = caster.target;
			count = 15;
			SetTime(2.0f);
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
	}
}