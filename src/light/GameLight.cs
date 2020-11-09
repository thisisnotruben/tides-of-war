using System.Collections.Generic;
using Godot;
namespace Game.Light
{
	public abstract class GameLight : Node2D
	{
		private static readonly HashSet<GameLight> gameLights = new HashSet<GameLight>();

		public override void _Ready() { gameLights.Add(this); }
		public static HashSet<GameLight> GetLights() { return gameLights; }
		public abstract void Start();
		public abstract void Stop();
		public abstract void SetIntensity(bool full, float min = 0.5f, float max = 1.0f);
	}
}