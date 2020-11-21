using Godot;
namespace Game.Light
{
	public class TorchLight : GameLight
	{
		public const float MAX_BRIGHT = 1.0f,
			MIN_BRIGHT = 0.8f;

		protected AnimationPlayer anim;
		protected Tween tween;
		protected Light2D light;
		protected VisibilityNotifier2D visibility;

		public override void _Ready()
		{
			base._Ready();
			anim = GetNode<AnimationPlayer>("anim");
			tween = GetNode<Tween>("tween");
			light = GetNode<Light2D>("light");
			visibility = GetNode<VisibilityNotifier2D>("visibility");

			Globals.TryLinkSignal(visibility, "screen_entered", this, nameof(Start), true);
			Globals.TryLinkSignal(visibility, "screen_exited", this, nameof(Stop), true);
		}
		public override void SetIntensity(bool maxBrightness)
		{
			float brightness = maxBrightness ? MAX_BRIGHT : MIN_BRIGHT;

			if (visibility.IsOnScreen())
			{
				tween.InterpolateProperty(light, "energy", light.Energy,
					brightness, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.In);
				tween.Start();
			}
			else
			{
				light.Energy = brightness;
			}
		}
		public override void Start()
		{
			if (!visibility.IsOnScreen())
			{
				return;
			}

			anim.Play("torch");
			light.Enabled = true;
			foreach (Node node in GetChildren())
			{
				Emit(node, true);
				foreach (Node childNode in node.GetChildren())
				{
					Emit(childNode, true);
				}
			}
		}
		public override void Stop()
		{
			if (visibility.IsOnScreen())
			{
				return;
			}

			anim.Stop();
			light.Enabled = false;
			foreach (Node node in GetChildren())
			{
				Emit(node, false);
				foreach (Node childNode in node.GetChildren())
				{
					Emit(childNode, false);
				}
			}
		}
		public void Emit(Node node, bool set)
		{
			Particles2D particles2D = node as Particles2D;
			if (particles2D != null)
			{
				particles2D.Emitting = set;
			}
		}
	}
}