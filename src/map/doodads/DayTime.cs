using Game.Light;
using Godot;
namespace Game.Map.Doodads
{
	public class DayTime : Timer, ISerializable
	{
		private const float LENGTH_OF_DAY = 210.0f;
		private bool dayLight = true;

		public void _OnTimerTimeout()
		{
			AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
			string animName = "sun_up_down";
			if (dayLight)
			{
				anim.Play(animName);
			}
			else
			{
				anim.PlayBackwards(animName);
			}
			dayLight = !dayLight;
		}
		public void _OnAnimFinished(string animName)
		{
			WaitTime = LENGTH_OF_DAY;
			Start();
		}
		public void TriggerLights()
		{
			foreach (GameLight light in GameLight.GetLights())
			{
				light.SetIntensity(!dayLight);
			}
		}
		public void Deserialize(Godot.Collections.Dictionary payload)
		{
			// TODO
		}
		public Godot.Collections.Dictionary Serialize()
		{
			// TODO
			return new Godot.Collections.Dictionary();
		}
	}
}