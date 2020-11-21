using Godot;
namespace Game.Light
{
	public abstract class GameLight : Node2D
	{
		public override void _Ready() { AddToGroup(Globals.LIGHT_GROUP); }
		public abstract void Start();
		public abstract void Stop();
		public abstract void SetIntensity(bool maxBrightness);
	}
}