using Godot;
namespace Game.Utils
{
	public class Speaker : AudioStreamPlayer
	{
		public void Delete()
		{
			QueueFree();
		}
	}
}