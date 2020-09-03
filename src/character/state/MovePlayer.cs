using Godot;
using Game.Ui;
namespace Game.Actor.State
{
	public class MovePlayer : Move
	{
		private static readonly PackedScene moveCursorScene = (PackedScene)GD.Load("res://src/menu_ui/cursor/MoveCursorView.tscn");
		[Signal]
		public delegate void PositionChanged();

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
			if (!Map.Map.map.IsValidMove(desiredPosition))
			{
				return;
			}

			// if desired position changed while we are moving
			if (tween.IsActive())
			{
				Map.Map.map.ResetPath(reservedPath);
				reservedPath.Clear();
				tween.RemoveAll();
			}

			path = Map.Map.map.getAPath(character.GlobalPosition, desiredPosition);
			MoveTo(path);

			// set cursor animation
			EmitSignal(nameof(PositionChanged));
			MoveCursorController cursor = (MoveCursorController)moveCursorScene.Instance();
			Connect(nameof(PositionChanged), cursor, nameof(MoveCursorController.Delete));
			cursor.AddToMap(Map.Map.map.GetGridPosition(desiredPosition));
		}
	}
}