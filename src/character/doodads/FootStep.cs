using Godot;
namespace Game.Actor.Doodads
{
	public class FootStep : Node2D
	{
		private const float STEP_FADE_DURATION = 2.0f;
		private static readonly Color BEGIN_COLOR = new Color("2e1a1a1a"), END_COLOR = new Color("00262626");
		private static readonly Rect2 STEP = new Rect2(0.0f, 0.0f, 3.0f, 3.0f);

		public override void _Draw() { DrawRect(STEP, BEGIN_COLOR); }
		public void OnTimerTimeout()
		{
			Tween tween = GetNode<Tween>("tween");
			tween.InterpolateProperty(this, ":modulate", Modulate,
				END_COLOR, STEP_FADE_DURATION,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		public void Delete() { QueueFree(); }
	}
}