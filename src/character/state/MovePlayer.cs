using Godot;
using Game.Database;
using Game.Ui;
namespace Game.Actor.State
{
	public class MovePlayer : Move
	{
		protected bool wantsToMove;
		[Signal] public delegate void PositionChanged();

		public override void Start()
		{
			base.Start();
			if (wantsToMove)
			{
				GetPathWay();
			}
			else
			{
				fsm.ChangeState(
					fsm.IsDead()
					? FSM.State.IDLE_DEAD
					: FSM.State.IDLE);
			}
		}
		public override void Exit()
		{
			base.Exit();
			wantsToMove = false;
		}
		public void OnPlayerWantsToMove(bool wantsToMove) { this.wantsToMove = wantsToMove; }
		public override void UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventScreenTouch && @event.IsPressed())
			{
				GetPathWay();
			}
		}
		public void GetPathWay()
		{
			Vector2 desiredPosition = character.GetGlobalMousePosition();
			if (!Map.Map.map.IsValidMove(character.GlobalPosition, desiredPosition))
			{
				return;
			}

			// if desired position changed while we are moving
			if (moving)
			{
				moving = false;
			}

			ResetPoint();
			path = Map.Map.map.getAPath(character.GlobalPosition, desiredPosition);
			MoveTo(path);

			// set cursor animation
			EmitSignal(nameof(PositionChanged));
			MoveCursorController cursor = (MoveCursorController)SceneDB.moveCursor.Instance();
			Connect(nameof(PositionChanged), cursor, nameof(MoveCursorController.Delete));
			cursor.AddToMap(Map.Map.map.GetGridPosition(desiredPosition));
		}
		protected override void OnMoveAnomaly(MoveAnomalyType moveAnomalyType) { fsm.ChangeState(fsm.IsDead() ? FSM.State.IDLE_DEAD : FSM.State.IDLE); }
	}
}