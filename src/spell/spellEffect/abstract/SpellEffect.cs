using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public abstract class SpellEffect : WorldObject
	{
		[Export] protected float lightFadeDelay = 0.65f;
		[Export] protected bool fadeLight = true;
		[Export] protected bool playSound = false;

		public Vector2 seekPos = Vector2.Zero; // only meteor uses this
		protected Character character;
		protected Tween tween;
		protected Timer timer;
		protected Sprite light;
		protected Node2D idleParticles, explodeParticles;

		protected delegate void Routine();
		protected Routine onHitEffect, onTimeOut;

		public virtual void Init(Character character, string spellWorldName)
		{
			this.character = character;

			switch (spellWorldName)
			{
				case WorldNameDB.CLEAVE:
				case WorldNameDB.DEVASTATE:
				case WorldNameDB.HASTE:
				case WorldNameDB.HEMORRHAGE:
				case WorldNameDB.OVERPOWER:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.img.Position;
						tween.Start();
						timer.Start();
					};

					break;
				case WorldNameDB.FIREBALL:
				case WorldNameDB.ARCANE_BOLT:
				case WorldNameDB.FROST_BOLT:
				case WorldNameDB.METEOR:
				case WorldNameDB.SHADOW_BOLT:
				case WorldNameDB.SIPHON_MANA:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Node2D bolt = idleParticles.GetNode<Node2D>("bolt");
						tween.InterpolateProperty(bolt, ":modulate", bolt.Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.In);
						tween.Start();
						timer.Start();

						if (spellWorldName.Equals(WorldNameDB.SIPHON_MANA))
						{
							GlobalPosition = character.target.pos;
						}
					};

					break;
				case WorldNameDB.CONCUSSIVE_SHOT:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						light.Show();
						PackedScene bashScene = (PackedScene)GD.Load("res://src/spell/effects/bash.tscn");
						// bash_effect bash = (bash_effect)bashScene.Instance();
						// character.target.AddChild(bash);
						// bash.Owner = character.target;
						// spell.Connect(nameof(Unmake), bash, nameof(bash_effect._OnTimerTimeout));
						// bash.GetNode<AudioStreamPlayer2D>("snd").Stream = null;
						// bash.OnHit();
						tween.Start();
						timer.Start();
					};

					break;
				case WorldNameDB.EXPLOSIVE_ARROW:
				case WorldNameDB.SNIPER_SHOT:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						light.Show();
						tween.Start();
						timer.Start();
					};

					break;
				case WorldNameDB.FRENZY:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position;
						character.GetParent().MoveChild(this, 0);
						tween.Start();
						timer.Start();
					};

					onTimeOut = () =>
					{
						// base._OnTimerTimeout();
						FadeLight(true);
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
						timer.Start();
					};

					break;
				case WorldNameDB.INTIMIDATING_SHOUT:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						Position = character.head.Position + new Vector2(0.0f, 6.0f);
						tween.Start();
						timer.Start();
					};

					onTimeOut = () =>
					{
						FadeLight(true);
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
						timer.Start();
					};

					break;
				case WorldNameDB.MIND_BLAST:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						// Bolt bolt = Owner as Bolt;
						// if (bolt != null)
						// {
						// 	bolt.GlobalPosition = bolt.target.head.GlobalPosition;
						// 	tween.Start();
						// 	timer.Start();
						// }
						// else
						// {
						// 	GD.Print("Owner not bolt in class MindBlast");
						// }
					};

					break;
				case WorldNameDB.STOMP:
				case WorldNameDB.SEARING_ARROW:
				case WorldNameDB.PIERCING_SHOT:
				case WorldNameDB.PRECISE_SHOT:
				case WorldNameDB.STINGING_SHOT:
				case WorldNameDB.BASH:
				case WorldNameDB.DIVINE_HEAL:
				case WorldNameDB.FORTIFY:

					onHitEffect = () =>
					{
						// base.OnHit(spell);
						tween.Start();
						timer.Start();

						switch (spellWorldName)
						{
							case WorldNameDB.BASH:
							case WorldNameDB.DIVINE_HEAL:
							case WorldNameDB.FORTIFY:
								Position = character.head.Position;
								break;
						}
					};

					switch (spellWorldName)
					{
						case WorldNameDB.BASH:
						case WorldNameDB.DIVINE_HEAL:
						case WorldNameDB.FORTIFY:
							onTimeOut = () =>
						   {
							   // base._OnTimerTimeout();
							   FadeLight(true);
							   tween.InterpolateProperty(this, ":modulate", Modulate,
								   new Color("00ffffff"), lightFadeDelay,
								   Tween.TransitionType.Linear, Tween.EaseType.InOut);
							   tween.Start();
							   timer.Start();
							   foreach (Particles2D particles2D in idleParticles.GetChildren())
							   {
								   particles2D.Emitting = false;
							   }
						   };
							break;
					}

					break;
				case WorldNameDB.SLOW:

					onHitEffect = () =>
					{
						// base._OnTimerTimeout();
						// Bolt bolt = Owner as Bolt;
						// if (bolt != null)
						// {
						// 	GetParent().RemoveChild(this);
						// 	bolt.target.AddChild(this);
						// 	Position = bolt.target.img.Position;
						// 	spell.Connect(nameof(Unmake), this, nameof(_OnTimerTimeout));
						// 	tween.Start();
						// 	timer.Start();
						// }
						// else
						// {
						// 	GD.Print("Unexpected parent in class: Slow");
						// }
					};

					onTimeOut = () =>
					{
						// base._OnTimerTimeout();
						FadeLight(true);
						foreach (Particles2D particles2D in idleParticles.GetChildren())
						{
							particles2D.Emitting = false;
						}
						timer.Start();
					};

					break;
				default:
					break;
			}
		}
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

			// SetProcess(false);
			// onTimeOut?.Invoke();
		}
		public virtual void _OnTimerTimeout() { /* QueueFree(); */ }
		// public override void _Process(float delta)
		// {
		// only meteor uses this
		// tween.InterpolateProperty(this, ":global_position", GlobalPosition,
		// 	seekPos, 5.0f, Tween.TransitionType.Circ, Tween.EaseType.Out);
		// tween.Start();
		// if (GlobalPosition.DistanceTo(seekPos) < 2.0f)
		// {
		// 	SetProcess(false);
		// 	OnHit();
		// }
		// }
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
			onHitEffect?.Invoke();
		}
		public void OnHit()
		{
			OnHit(null);
		}
	}
}