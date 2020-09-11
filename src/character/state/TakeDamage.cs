using System;
using Godot;
namespace Game.Actor.State
{
	public abstract class TakeDamage : StateBehavior
	{
		private Tween tween = new Tween();

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
		}
		public virtual void Harm(int damage)
		{
			damage -= character.stats.armor.valueI;
			if (damage <= 0)
			{
				return;
			}

			character.hp -= damage;

			// TODO: causing move problems, maybe do a camera shake
			// if (character.target != null && !tween.IsActive() && !fsm.IsMoving())
			// {
			// 	Bump(Map.Map.map.GetDirection(character.GlobalPosition,
			// 		character.target.GlobalPosition).Rotated((float)Math.PI) / 4.0f);
			// }
		}
		private protected async void Bump(Vector2 direction)
		{
			if (direction.Equals(Vector2.Zero))
			{
				return;
			}

			// bump
			tween.InterpolateProperty(character, ":global_position",
				character.GlobalPosition,
				character.GlobalPosition + direction,
				character.GlobalPosition.DistanceTo(character.GlobalPosition + direction) / 10.0f,
				Tween.TransitionType.Elastic,
				Tween.EaseType.Out);
			tween.Start();

			// wait for bump animation to finish
			await ToSignal(tween, "tween_completed");

			// return to center tile
			Vector2 gridPos = Map.Map.map.GetGridPosition(character.GlobalPosition);
			tween.InterpolateProperty(character, ":global_position",
				character.GlobalPosition,
				gridPos,
				gridPos.DistanceTo(character.GlobalPosition) / 10.0f,
				Tween.TransitionType.Elastic,
				Tween.EaseType.In);
			tween.Start();
		}
	}
}