using Game.Actor;
using Godot;
namespace Game.Ui
{
	public class MoveCursor : Node2D
	{
		public void _OnAnimFinished(string AnimName)
		{
			Delete();
		}
		public void AddToMap(Player originator, Vector2 globalTilePosition)
		{
			originator.Connect(nameof(Player.PosChanged), this, nameof(Delete));
			Map.Map.map.GetNode("ground").AddChild(this);
			GlobalPosition = globalTilePosition;
		}
		public void Delete()
		{
			QueueFree();
		}
	}
}