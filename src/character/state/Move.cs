using System.Collections.Generic;
using Godot;
namespace Game.Actor.State
{
	public abstract class Move : TakeDamage
	{
		public enum MoveAnomalyType { INVALID_PATH, OBSTRUCTION_DETECTED }

		protected Tween tween = new Tween();

		private Queue<Vector2> reservedPath = new Queue<Vector2>();
		protected Queue<Vector2> _path = new Queue<Vector2>();
		protected Queue<Vector2> path
		{
			get { return _path; }
			set
			{
				ResetPath();
				_path = value;
			}
		}

		public override void _Ready()
		{
			base._Ready();
			AddChild(tween);
			tween.PlaybackProcessMode = Tween.TweenProcessMode.Physics;
			tween.Connect("tween_completed", this, nameof(_OnTweenCompleted));
			tween.Connect("tween_completed", this, nameof(_OnMoveCompleted));
		}
		public override void Start()
		{
			character.img.FlipH = false;
			character.anim.Play("moving", -1, character.stats.animSpeed.value);
			character.anim.Seek(0.33f, true);
		}
		public override void Exit()
		{
			// stop all movement
			tween.StopAll();
			character.anim.Stop();
			character.img.Frame = 0;
			path.Clear();
		}
		protected void ResetPath()
		{
			if (reservedPath.Count > 0)
			{
				if (fsm.IsMoving())
				{
					Map.Map.map.OccupyCell(reservedPath.Peek(), false);
				}
				reservedPath.Dequeue();
			}
		}
		private void MoveCharacter(Vector2 targetPosition, float speedModifier = Stats.SPEED)
		{
			// tween.StopAll();
			tween.InterpolateProperty(character, ":global_position", character.GlobalPosition, targetPosition,
				character.GlobalPosition.DistanceTo(targetPosition) / 16.0f * speedModifier,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		private protected void MoveTo(Queue<Vector2> route)
		{
			if (route.Count == 0)
			{
				OnMoveAnomaly(MoveAnomalyType.INVALID_PATH);
				return;
			}

			Vector2 direction = Map.Map.map.GetDirection(character.GlobalPosition, route.Peek());
			while (route.Count > 0 && direction.Equals(Vector2.Zero))
			{
				route.Dequeue();
				if (route.Count > 0)
				{
					direction = Map.Map.map.GetDirection(character.GlobalPosition, route.Peek());
				}
			}
			if (route.Count == 0)
			{
				OnMoveAnomaly(MoveAnomalyType.INVALID_PATH);
				return;
			}

			if (Map.Map.map.IsValidMove(route.Peek()))
			{
				Map.Map.map.OccupyCell(route.Peek(), true);
				reservedPath.Enqueue(route.Peek());

				MoveCharacter(route.Dequeue(), Stats.MapAnimMoveSpeed(character.stats.animSpeed.value));
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

				fsm.ChangeState(fsm.IsDead() ? FSM.State.IDLE_DEAD : FSM.State.IDLE);
			}
			else
			{
				MoveTo(path);
			}
		}
		private void _OnMoveCompleted(Godot.Object Gobject, NodePath nodePath) { ResetPath(); }
		protected virtual void OnMoveAnomaly(MoveAnomalyType moveAnomalyType) { }
	}
}