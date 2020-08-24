using System.Collections.Generic;
using Godot;
namespace Game.Actor.State
{
	public abstract class Move : TakeDamage
	{
		private protected Tween tween = new Tween();
		private protected List<Vector2> path = new List<Vector2>();
		private protected List<Vector2> reservedPath = new List<Vector2>();
		[Signal]
		public delegate void InvalidPath();
		[Signal]
		public delegate void ObstructionDetected();

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
			tween.Connect("tween_completed", this, nameof(_OnTweenCompleted));
		}
		public override void Start()
		{
			sprite.FlipH = false;
			animationPlayer.Play("moving", -1, character.animSpeed);
			animationPlayer.Seek(0.3f, true);
		}
		public override void Exit()
		{
			animationPlayer.Stop();
			sprite.Frame = 0;
		}
		private void MoveCharacter(Vector2 targetPosition, float speedModifier = Stats.SPEED)
		{
			tween.StopAll();
			tween.InterpolateProperty(character, ":global_position", character.GlobalPosition, targetPosition,
				character.GlobalPosition.DistanceTo(targetPosition) / 16.0f * speedModifier,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		private protected void MoveTo(List<Vector2> route)
		{
			if (route.Count == 0)
			{
				EmitSignal(nameof(InvalidPath));
			}
			Vector2 direction = Map.Map.map.GetDirection(character.GlobalPosition, route[0]);
			while (route.Count > 0 && direction.x == 0.0f && direction.y == 0.0f)
			{
				route.RemoveAt(0);
				if (route.Count > 0)
				{
					direction = Map.Map.map.GetDirection(character.GlobalPosition, route[0]);
				}
			}
			if (route.Count == 0)
			{
				EmitSignal(nameof(InvalidPath));
			}

			route[0] = Map.Map.map.RequestMove(character.GlobalPosition, direction);
			if (route[0].x != 0.0f && route[0].y != 0.0f)
			{
				reservedPath.Add(route[0]);
				MoveCharacter(route[0], Stats.MapAnimMoveSpeed(character.animSpeed));
				route.RemoveAt(0);
			}
			else
			{
				EmitSignal(nameof(ObstructionDetected));
			}
		}
		public virtual void _OnTweenCompleted(Godot.Object Gobject, NodePath nodePath)
		{
			if (path.Count == 0)
			{
				fsm.ChangeState(FSM.State.IDLE);
			}
			else
			{
				MoveTo(path);
			}
		}
	}
}