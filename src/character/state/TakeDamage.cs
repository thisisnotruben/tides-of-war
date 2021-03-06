using Godot;
using System;
namespace Game.Actor.State
{
	public abstract class TakeDamage : StateBehavior
	{
		private Tween tween = new Tween();
		private bool bumpReturn;

		public override void _Ready()
		{
			base._Ready();
			tween.Connect("tween_all_completed", this, nameof(OnTweenAllCompleted));
			AddChild(tween);
		}
		public virtual void Harm(int damage, Vector2 direction)
		{
			damage -= character.stats.armor.valueI;
			if (damage <= 0)
			{
				return;
			}

			character.hp -= damage;

			if (!direction.Equals(Vector2.Zero) && !tween.IsActive() && !fsm.IsMoving())
			{
				Bump(Map.Map.map.GetDirection(character.GlobalPosition,
					direction).Rotated((float)Math.PI) / 4.0f);
			}
		}
		protected void Bump(Vector2 direction)
		{
			bumpReturn = false;
			Node2D img = character.img;

			tween.InterpolateProperty(img, "global_position",
				img.GlobalPosition,
				img.GlobalPosition + direction,
				img.GlobalPosition.DistanceTo(img.GlobalPosition + direction) / 10.0f,
				Tween.TransitionType.Elastic,
				Tween.EaseType.Out);
			tween.Start();
		}
		public void OnTweenAllCompleted()
		{
			if (bumpReturn)
			{
				return;
			}

			bumpReturn = true;

			// return to center tile
			Node2D img = character.img;
			Vector2 gridPos = Map.Map.map.GetGridPosition(img.GlobalPosition);

			tween.InterpolateProperty(img, "global_position",
				img.GlobalPosition,
				gridPos,
				gridPos.DistanceTo(img.GlobalPosition) / 10.0f,
				Tween.TransitionType.Elastic,
				Tween.EaseType.In);
			tween.Start();
		}
	}
}