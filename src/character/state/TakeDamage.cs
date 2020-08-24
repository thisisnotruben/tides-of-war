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
			damage -= character.armor;
			if (damage <= 0)
			{
				return;
			}

			character.hp = -damage;

			if (!tween.IsActive() && fsm.GetState() != FSM.State.MOVE)
			{
				Bump(Map.Map.map.GetDirection(character.GlobalPosition,
					character.target.GlobalPosition).Rotated((float)Math.PI) / 4.0f);
			}

			Player player = character as Player;
			if (player != null && player.vest != null)
			{
				player.vest.TakeDamage(false);
			}
		}
		private protected async void Bump(Vector2 direction)
		{
			if (direction.x == 0.0f && direction.y == 0.0f)
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