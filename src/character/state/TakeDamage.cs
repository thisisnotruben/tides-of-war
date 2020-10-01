using Godot;
namespace Game.Actor.State
{
	public abstract class TakeDamage : StateBehavior
	{
		private Tween tween = new Tween();
		private bool bumpReturn;

		public override void _Ready()
		{
			base._Ready();
			tween.Connect("tween_completed", this, nameof(OnTweenCompleted));
			AddChild(tween);
		}
		public virtual void Harm(int damage)
		{

			// TODO: damage -= character.stats.armor.valueI;
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
		private protected void Bump(Vector2 direction)
		{
			if (direction.Equals(Vector2.Zero))
			{
				return;
			}

			bumpReturn = false;

			// bump
			tween.InterpolateProperty(character, ":global_position",
				character.GlobalPosition,
				character.GlobalPosition + direction,
				character.GlobalPosition.DistanceTo(character.GlobalPosition + direction) / 10.0f,
				Tween.TransitionType.Elastic,
				Tween.EaseType.Out);
			tween.Start();
		}
		public void OnTweenCompleted(Godot.Object gObject, NodePath key)
		{
			if (bumpReturn)
			{
				return;
			}

			bumpReturn = true;

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