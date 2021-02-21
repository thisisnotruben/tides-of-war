using System.Collections.Generic;
using Godot;
namespace Game.Actor.State
{
	public abstract class Move : TakeDamage
	{
		public enum MoveAnomalyType { INVALID_PATH, OBSTRUCTION_DETECTED }
		public const float CHARACTER_SPEED = 32.0f, MIN_ARRIVAL_DIST = 1.0f;

		public bool moving
		{
			get { return IsPhysicsProcessing(); }
			set { SetPhysicsProcess(value); }
		}
		private Vector2 targetPosition = Vector2.Zero;
		private Queue<Vector2> reservedPath = new Queue<Vector2>();
		protected Queue<Vector2> _path = new Queue<Vector2>();
		protected Queue<Vector2> path
		{
			get { return _path; }
			set
			{
				while (reservedPath.Count > 0)
				{
					ResetPoint();
				}
				_path = value;
			}
		}

		public override void _Ready()
		{
			base._Ready();
			SetPhysicsProcess(false);
		}
		public override void Start()
		{
			character.img.FlipH = false;
			character.anim.Play("moving", -1, character.stats.animSpeed.value);
			character.anim.Seek(0.33f, true);
		}
		public override void Exit()
		{
			SetPhysicsProcess(false);
			character.anim.Stop();
			character.img.Frame = 0;
			path.Clear();
		}
		public override void OnAttacked(Character whosAttacking) { ClearOnAttackedSignals(whosAttacking); }
		protected void ResetPoint()
		{
			if (reservedPath.Count > 0)
			{
				if (fsm.IsMoving())
				{
					// we don't want to give up our space when in 'idle'
					Map.Map.map.OccupyCell(reservedPath.Peek(), false);
				}
				reservedPath.Dequeue();
			}
		}
		public override void _PhysicsProcess(float delta)
		{
			if (character.GlobalPosition.DistanceTo(targetPosition) < MIN_ARRIVAL_DIST)
			{
				ResetPoint();
				OnMovePointFinished();
			}
			else
			{
				character.GlobalPosition += character.GlobalPosition.DirectionTo(targetPosition) * CHARACTER_SPEED * delta;
			}
		}
		protected void MoveTo(Queue<Vector2> route)
		{
			if (route.Count == 0)
			{
				OnMoveAnomaly(MoveAnomalyType.INVALID_PATH);
				return;
			}

			if (Map.Map.map.IsValidMove(character.GlobalPosition, route.Peek()))
			{
				Map.Map.map.OccupyCell(route.Peek(), true);
				reservedPath.Enqueue(route.Peek());

				MoveCharacter(route.Dequeue());
			}
			else
			{
				OnMoveAnomaly(MoveAnomalyType.OBSTRUCTION_DETECTED);
			}
		}
		private void MoveCharacter(Vector2 targetPosition)
		{
			this.targetPosition = targetPosition;
			SetPhysicsProcess(true);
		}
		protected virtual void OnMovePointFinished()
		{
			if (path.Count == 0)
			{
				fsm.ChangeState(fsm.IsDead() ? FSM.State.IDLE_DEAD : FSM.State.IDLE);
			}
			else
			{
				MoveTo(path);
			}
		}
		protected virtual void OnMoveAnomaly(MoveAnomalyType moveAnomalyType) { }
	}
}