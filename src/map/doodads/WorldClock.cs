using Game.Light;
using Game.Database;
using Godot;
using GC = Godot.Collections;
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
			// * TODO: waiting for godot 4.0 to fix lights, huge fps drop
			// Globals.TryLinkSignal(anim, "animation_finished", this, nameof(OnAnimFinished), true);
			// Globals.TryLinkSignal(this, "timeout", this, nameof(OnTimerTimeout), true);
			// AddToGroup(Globals.SAVE_GROUP);
		}
		public void OnTimerTimeout() { anim.Play(ANIM_NAME, -1.0f, dayLight ? 1.0f : -1.0f, !dayLight); }
		public void OnAnimFinished(string animName)
		{
			WaitTime = LENGTH_OF_DAY;
			dayLight = !dayLight;
			Start();
		}
		public void TriggerLights() // called from animation
		{
			foreach (Node light in GetTree().GetNodesInGroup(Globals.LIGHT_GROUP))
			{
				(light as LightSource)?.DimLight(dayLight);
			}
		}
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.DAY_LIGHT, dayLight},
				{NameDB.SaveTag.TIME_LEFT, TimeLeft},
				{NameDB.SaveTag.ANIM_POSITION, !anim.CurrentAnimation.Equals(string.Empty) ? anim.CurrentAnimationPosition : 0.0f}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			dayLight = (bool)payload[NameDB.SaveTag.DAY_LIGHT];
			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT],
				animPos = (float)payload[NameDB.SaveTag.ANIM_POSITION];

			if (timeLeft > 0.0f)
			{
				Start(timeLeft);
			}
			else
			{
				OnTimerTimeout();
				if (animPos > 0.0f && animPos < anim.GetAnimation(ANIM_NAME).Length)
				{
					anim.Advance(animPos);
				}
			}
		}
	}
}