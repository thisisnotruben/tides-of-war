using Godot;
namespace Game.Light
{
	public class LightSource : Light2D
	{
		[Export] public Gradient gradient;
		[Export(PropertyHint.Range, "0.0,1.0")]
		public float
		   gradientSpeed = 1.0f,
		   minBrightness = 0.75f,
		   maxBrightness = 1.0f;
		[Export] public float dimSpeed = 2.0f;

		private Tween tween;
		private VisibilityNotifier2D visibilityNotifier2D;
		private int i = 0, direction = 1;

		public override void _Ready()
		{
			// * TOOD: waiting for godot 4.0 to fix lights, huge fps drop
			Enabled = false;

			tween = GetChild<Tween>(0);
			// tween.Connect("tween_all_completed", this, nameof(OnTweenAllCompleted));

			Vector2 textureSize = Texture.GetSize() * TextureScale;

			visibilityNotifier2D = GetChild<VisibilityNotifier2D>(1);
			visibilityNotifier2D.Rect = new Rect2(textureSize / -2.0f, textureSize);
			// visibilityNotifier2D.Connect("screen_entered", this, nameof(OnScreenEnteredExited));
			// visibilityNotifier2D.Connect("screen_exited", this, nameof(OnScreenEnteredExited));

			// StartTween();
			AddToGroup(Globals.LIGHT_GROUP);
		}
		private void OnTweenAllCompleted() { /* StartTween(); */ }
		private void OnScreenEnteredExited() { /* tween.SetActive(visibilityNotifier2D.IsOnScreen()); */ }
		private void StartTween()
		{
			// loop through gradient colors
			int next = (i + direction) % gradient.Colors.Length;
			if (next == 0)
			{
				direction *= -1;
			}
			if (direction == -1)
			{
				next = gradient.Colors.Length - 1 - next;
			}

			tween.InterpolateProperty(this, "color",
				gradient.Colors[i], gradient.Colors[next],
				gradientSpeed, Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();

			OnScreenEnteredExited();

			i = next;
		}
		public void DimLight(bool dimDown)
		{
			ShadowEnabled = dimDown;
			Energy = dimDown ? 0.75f : 0.65f;
		}
	}
}