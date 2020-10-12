using Godot;
using Game.Ui;
namespace Game.Actor.State
{
	public class MovePlayer : Move
	{
		[Signal] public delegate void PositionChanged();

		public override void Start()
		{
			base.Start();
			GetPathWay();
		}
		public override void UnhandledInput(InputEvent @event)
		{
			if (!(@event is InputEventScreenTouch))
			{
				return;
			}
			else if (!@event.IsPressed() || @event.IsEcho())
			{
				return;
			}
			GetPathWay();
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

			path = Map.Map.map.getAPath(character.GlobalPosition, desiredPosition);
			MoveTo(path);

			// set cursor animation
			EmitSignal(nameof(PositionChanged));
			MoveCursorController cursor = (MoveCursorController)MoveCursorController.scene.Instance();
			Connect(nameof(PositionChanged), cursor, nameof(MoveCursorController.Delete));
			cursor.AddToMap(Map.Map.map.GetGridPosition(desiredPosition));
		}
		protected override void OnMoveAnomaly(MoveAnomalyType moveAnomalyType) { fsm.ChangeState(fsm.IsDead() ? FSM.State.IDLE_DEAD : FSM.State.IDLE); }
	}
}