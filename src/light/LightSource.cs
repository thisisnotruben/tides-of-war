using Godot;
namespace Game.Light
{
	public class LightSource : Light2D
	{
		[Export] public Gradient gradient;
		[Export] public float speed = 1.0f;

		private Tween tween;
		private VisibilityNotifier2D visibilityNotifier2D;
		private int i = 0, direction = 1;

		public override void _Ready()
		{
			tween = GetChild<Tween>(0);
			visibilityNotifier2D = GetChild<VisibilityNotifier2D>(1);

			Vector2 textureSize = Texture.GetSize() * TextureScale;
			visibilityNotifier2D.Rect = new Rect2(textureSize / -2.0f, textureSize);
			StartTween();
		}
		public void OnTweenAllCompleted() { StartTween(); } // connected from scene
		public void OnScreenEnteredExited() { tween.SetActive(visibilityNotifier2D.IsOnScreen()); }
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
				speed, Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();

			OnScreenEnteredExited();

			i = next;
		}
	}
}