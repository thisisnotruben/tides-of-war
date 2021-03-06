using Godot;
namespace Game.Ui
{
	public class MoveCursorController : Node2D
	{
		public void AddToMap(Vector2 globalTilePosition)
		{
			Map.Map.map.AddGChild(this);
			GlobalPosition = globalTilePosition;
		}
		public void Delete() { QueueFree(); }
	}
}