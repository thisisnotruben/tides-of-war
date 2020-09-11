using System.Collections.Generic;
using Godot;
namespace Game.Actor.State
{
	public abstract class Move : TakeDamage
	{
		public enum MoveAnomalyType { INVALID_PATH, OBSTRUCTION_DETECTED }
		private protected Tween tween = new Tween();
		private protected List<Vector2> path = new List<Vector2>();
		private protected List<Vector2> reservedPath = new List<Vector2>();

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
			tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
			tween.Connect("tween_completed", this, nameof(_OnTweenCompleted));
		}
		public override void Start()
		{
			character.img.FlipH = false;
			character.anim.Play("moving", -1, character.stats.animSpeed.value);
			character.anim.Seek(0.3f, true);
		}
		public override void Exit()
		{
			// stop all movement
			tween.StopAll();
			character.anim.Stop();
			character.img.Frame = 0;
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
				OnMoveAnomaly(MoveAnomalyType.INVALID_PATH);
				return;
			}
			Vector2 direction = Map.Map.map.GetDirection(character.GlobalPosition, route[0]);
			while (route.Count > 0 && direction.Equals(Vector2.Zero))
			{
				route.RemoveAt(0);
				if (route.Count > 0)
				{
					direction = Map.Map.map.GetDirection(character.GlobalPosition, route[0]);
				}
			}
			if (route.Count == 0)
			{
				OnMoveAnomaly(MoveAnomalyType.INVALID_PATH);
				return;
			}

			route[0] = Map.Map.map.RequestMove(character.GlobalPosition, direction);
			if (!route[0].Equals(Vector2.Zero))
			{
				reservedPath.Add(route[0]);
				MoveCharacter(route[0], Stats.MapAnimMoveSpeed(character.stats.animSpeed.value));
				route.RemoveAt(0);
			}
			else
			{
				OnMoveAnomaly(MoveAnomalyType.OBSTRUCTION_DETECTED);
			}
		}
		public virtual void _OnTweenCompleted(Godot.Object Gobject, NodePath nodePath)
		{
			if (path.Count == 0)
			{
				// makes sure player can combat with enemies within range
				(character as Player)?.OnAttacked(character.target);
				if (!fsm.IsMoving())
				{
					// then we are atacking and ignore rest
					return;
				}

				fsm.ChangeState((fsm.IsDead()) ? FSM.State.IDLE_DEAD : FSM.State.IDLE);
			}
			else
			{
				MoveTo(path);
			}
		}
		private protected virtual void OnMoveAnomaly(MoveAnomalyType moveAnomalyType) { }
	}
}