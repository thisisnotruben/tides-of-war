using Godot;
namespace Game.Light
{
	public abstract class GameLight : Node2D
	{
		public override void _Ready() { AddToGroup(Globals.LIGHT_GROUP); }
		public abstract void Start();
		public abstract void Stop();
		public abstract void SetIntensity(bool full, float min = 0.5f, float max = 1.0f);
	}
}