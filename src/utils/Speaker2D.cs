using Godot;
namespace Game.Utils
{
	public class Speaker2D : AudioStreamPlayer2D
	{
		public void Delete()
		{
			QueueFree();
		}
	}
}