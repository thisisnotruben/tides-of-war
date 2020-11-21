using Game.Light;
using Godot;
namespace Game.Map.Doodads
{
	public class DayTime : Timer, ISerializable
	{
		private const float LENGTH_OF_DAY = 210.0f;
		private const string ANIM_NAME = "SunUpDown";
		private bool dayLight = true;
		private AnimationPlayer anim;

		public override void _Ready()
		{
			if (!IsConnected("timeout", this, nameof(_OnTimerTimeout)))
			{
				Connect("timeout", this, nameof(_OnTimerTimeout));
			}
			anim = GetNode<AnimationPlayer>("anim");
		}
		public void _OnTimerTimeout()
		{
			anim.Play(ANIM_NAME, -1.0f, dayLight ? 1.0f : -1.0f, !dayLight);
			dayLight = !dayLight;
		}
		public void _OnAnimFinished(string animName)
		{
			WaitTime = LENGTH_OF_DAY;
			Start();
		}
		public void TriggerLights()
		{
			foreach (GameLight light in GetTree().GetNodesInGroup(Globals.LIGHT_GROUP))
			{
				light.SetIntensity(!dayLight);
			}
		}
		public void Deserialize(Godot.Collections.Dictionary payload)
		{
			dayLight = (bool)payload["dayLight"];

			if ((bool)payload["animPlaying"])
			{
				anim.Play(ANIM_NAME, -1.0f, (dayLight) ? 1.0f : -1.0f, !dayLight);
				anim.Seek((float)payload["animPosition"]);
			}
			else
			{
				Stop();
				WaitTime = (float)payload["timerPosition"];
				Start();
			}
		}
		public Godot.Collections.Dictionary Serialize()
		{
			return new Godot.Collections.Dictionary()
			{
				{"animPosition", anim.CurrentAnimationPosition},
				{"animPlaying", anim.IsPlaying()},
				{"timerPosition", TimeLeft},
				{"dayLight", dayLight}
			};
		}
	}
}