using Godot;
namespace Game.Ui
{
	public class MoveCursorController : Node2D
	{
		public static readonly PackedScene scene = (PackedScene)GD.Load("res://src/menu_ui/cursor/MoveCursorView.tscn");

		public void AddToMap(Vector2 globalTilePosition)
		{
			Map.Map.map.GetNode("ground").AddChild(this);
			GlobalPosition = globalTilePosition;
		}
		public void _OnAnimFinished(string AnimName) { Delete(); }
		public void Delete() { QueueFree(); }
	}
}