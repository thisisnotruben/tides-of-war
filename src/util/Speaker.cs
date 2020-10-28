using Godot;
namespace Game.Util
{
	public class Speaker : AudioStreamPlayer
	{
		public void Delete()
		{
			QueueFree();
		}
	}
}