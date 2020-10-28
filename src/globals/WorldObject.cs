using Godot;
namespace Game
{
	public abstract class WorldObject : Node2D
	{
		public string worldName { get; protected set; }
	}
}