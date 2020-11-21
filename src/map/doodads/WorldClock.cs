using Game.Light;
using Game.Database;
using Godot;
namespace Game.Map.Doodads
{
	public class WorldClock : Timer, ISerializable
	{
		public const float LENGTH_OF_DAY = 210.0f;
		public const string ANIM_NAME = "SunUpDown";

		protected bool dayLight = true;
		protected AnimationPlayer anim;

		public override void _Ready()
		{
			anim = GetNode<AnimationPlayer>("anim");
			Globals.TryLinkSignal(this, "timeout", this, nameof(OnTimerTimeout), true);
			AddToGroup(Globals.SAVE_GROUP);
		}
		public void OnTimerTimeout()
		{
			anim.Play(ANIM_NAME, -1.0f, dayLight ? 1.0f : -1.0f, !dayLight);
			dayLight = !dayLight;
		}
		public void OnAnimFinished(string animName) // connected from scene
		{
			WaitTime = LENGTH_OF_DAY;
			Start();
		}
		public void TriggerLights() // called from animation
		{
			foreach (GameLight light in GetTree().GetNodesInGroup(Globals.LIGHT_GROUP))
			{
				light.SetIntensity(!dayLight);
			}
		}
		public Godot.Collections.Dictionary Serialize()
		{
			return new Godot.Collections.Dictionary()
			{
				{NameDB.SaveTag.DAY_LIGHT, dayLight},
				{NameDB.SaveTag.TIME_LEFT, TimeLeft},
				{NameDB.SaveTag.POSITION, anim.CurrentAnimationPosition}
			};
		}
		public void Deserialize(Godot.Collections.Dictionary payload)
		{
			dayLight = (bool)payload[NameDB.SaveTag.DAY_LIGHT];
			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT],
				animPos = (float)payload[NameDB.SaveTag.POSITION];

			if (timeLeft > 0.0f)
			{
				Stop();
				WaitTime = timeLeft;
				Start();
			}
			else
			{
				anim.Play(ANIM_NAME, -1.0f, dayLight ? 1.0f : -1.0f, !dayLight);
				anim.Seek(animPos);
			}
		}
	}
}