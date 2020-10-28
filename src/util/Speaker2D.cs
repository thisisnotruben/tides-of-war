using Godot;
namespace Game.Util
{
	public class Speaker2D : AudioStreamPlayer2D
	{
		public void Delete()
		{
			QueueFree();
		}
	}
}