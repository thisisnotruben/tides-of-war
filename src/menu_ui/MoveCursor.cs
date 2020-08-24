using Godot;
namespace Game.Ui
{
	public class MoveCursor : Node2D
	{
		public void AddToMap(Vector2 globalTilePosition)
		{
			Map.Map.map.GetNode("ground").AddChild(this);
			GlobalPosition = globalTilePosition;
		}
		public void _OnAnimFinished(string AnimName) { Delete(); }
		public void Delete() { QueueFree(); }
	}
}