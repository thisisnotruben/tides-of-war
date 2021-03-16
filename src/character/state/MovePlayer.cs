using Godot;
using GC = Godot.Collections;
using Game.Database;
using Game.Ui;
namespace Game.Actor.State
{
	public class MovePlayer : Move
	{
		public Vector2 desiredPosition = Vector2.Zero;
		[Signal] public delegate void PositionChanged();

		public override void Start()
		{
			base.Start();
			if (desiredPosition.Equals(Vector2.Zero))
			{
				fsm.ChangeState(
					fsm.IsDead()
					? FSM.State.IDLE_DEAD
					: FSM.State.IDLE);
			}
			else
			{
				GetPathWay();
			}
		}
		public override void Exit()
		{
			base.Exit();
			desiredPosition = Vector2.Zero;
		}
		public void OnPlayerWantsToMove(Vector2 desiredPosition) { this.desiredPosition = desiredPosition; }
		public override void UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventScreenTouch && @event.IsPressed())
			{
				desiredPosition = character.GetGlobalMousePosition();
				GetPathWay();
			}
		}
		public void GetPathWay()
		{
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
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.POSITION] = new GC.Array() { desiredPosition.x, desiredPosition.y };
			return payload;
		}
		public override void Deserialize(GC.Dictionary payload) { EmitSignal(nameof(PositionChanged)); }
	}
}