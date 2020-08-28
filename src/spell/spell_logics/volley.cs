using Game.Actor.State;
using Godot;
namespace Game.Ability
{
	public class volley : Spell
	{
		private const float animSpeed = 1.5f;
		private const string animName = "attacking";
		public override float Cast()
		{
			// TODO: not sure how this class is going to work, need to test
			return base.Cast();
		}
		public void VolleyCast(string animName)
		{
			AnimationPlayer casterAnim = caster.anim;
			if (target == null)
			{
				if (!loaded)
				{
					caster.mana -= manaCost;
				}
				count = 3;
				target = caster.target;
				casterAnim.Play(animName, -1, animSpeed);
			}
			else
			{
				if (caster.pos.DistanceTo(target.pos) <= caster.stats.weaponRange.value)
				{
					count--;
					if (count > 0)
					{
						casterAnim.Play(animName, -1, animSpeed);
					}
				}
				else if (count < 0)
				{
					caster.GetNode<Timer>("timer").SetBlockSignals(false);
					caster.SetProcess(true);
					UnMake();
				}
			}
		}
		public override void ConfigureSpell()
		{
			caster.SetCurrentSpell(this);
			caster.state = FSM.State.IDLE;
			caster.SetProcess(false);
			caster.GetNode<Timer>("timer").SetBlockSignals(true);
			AnimationPlayer casterAnim = caster.anim;
			casterAnim.Connect("animation_finished", this, nameof(VolleyCast));
			casterAnim.Play(animName, -1.0f, animSpeed);
		}
	}
}