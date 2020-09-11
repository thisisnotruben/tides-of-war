using Game.Actor;
using Godot;
namespace Game.Ability
{
	public abstract class SpellEffect : WorldObject
	{
		[Export] protected float lightFadeDelay = 0.65f;
		[Export] protected bool fadeLight = true;
		[Export] protected bool playSound = false;
		public Vector2 seekPos = new Vector2();
		protected Character character;
		protected Tween tween;
		protected Timer timer;
		protected Sprite light;
		protected Node2D idleParticles, explodeParticles;

		public virtual void Init(Character character) { this.character = character; }
		public virtual void _OnTimerTimeout() { QueueFree(); }
		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			timer = GetNode<Timer>("timer");
			light = GetNode<Sprite>("light");
			idleParticles = GetNode<Node2D>("idle");
			explodeParticles = GetNode<Node2D>("explode");

			foreach (Node2D node2D in idleParticles.GetChildren())
			{
				node2D.UseParentMaterial = true;
			}
			foreach (Node2D node2D in explodeParticles.GetChildren())
			{
				node2D.UseParentMaterial = true;
			}

			SetProcess(false);
		}
		public override void _Process(float delta)
		{
			tween.InterpolateProperty(this, ":global_position", GlobalPosition,
				seekPos, 5.0f, Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
			if (GlobalPosition.DistanceTo(seekPos) < 2.0f)
			{
				SetProcess(false);
				OnHit();
			}
		}
		public void FadeLight(bool fade = true)
		{
			if (fade)
			{
				tween.InterpolateProperty(light, ":modulate", light.Modulate,
					new Color("00ffffff"), lightFadeDelay, Tween.TransitionType.Linear, Tween.EaseType.In);
			}
		}
		public virtual void OnHit(Spell spell = null)
		{
			GetNode<AudioStreamPlayer2D>("snd").Play();
			tween.InterpolateProperty(this, ":scale", new Vector2(0.75f, 0.75f),
				Vector2.One, 0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
			FadeLight(fadeLight);
			foreach (Particles2D particles2D in idleParticles.GetChildren())
			{
				particles2D.Emitting = false;
			}
			foreach (Particles2D particles2D in explodeParticles.GetChildren())
			{
				particles2D.Emitting = true;
			}
			// Call this method first in every polymorphed method
		}
	}
}