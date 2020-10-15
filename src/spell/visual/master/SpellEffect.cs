using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Ability
{
	public class SpellEffect : WorldObject
	{
		[Export] protected float lightFadeDelay = 0.65f;
		[Export] protected bool fadeLight = true;
		[Export] protected bool playSound = false; // for missile to play sound?

		private Character character;
		private Tween tween;
		private Timer timer;
		private Sprite light;
		private Node2D idleParticles, explodeParticles;
		private string sound;

		private delegate void Routine();
		private Routine onTimeOut;
		private readonly Stack<Routine> polyBehavior = new Stack<Routine>();

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
		}
		public void Init(Character character, string spellWorldName, Node attachTo)
		{
			this.character = character;
			sound = SpellDB.GetSpellData(spellWorldName).sound;

			attachTo.AddChild(this);
			Owner = attachTo;

			// default behavior
			AddBehavior(() =>
			{
				tween.Start();
				timer.Start();
			});

			// added behavior
			switch (spellWorldName)
			{
				case WorldNameDB.BASH:
				case WorldNameDB.DIVINE_HEAL:
				case WorldNameDB.FORTIFY:
					AddBehavior(() => Position = character.head.Position);
					onTimeOut = () =>
					{
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

				case WorldNameDB.CLEAVE:
				case WorldNameDB.DEVASTATE:
				case WorldNameDB.HASTE:
				case WorldNameDB.HEMORRHAGE:
				case WorldNameDB.OVERPOWER:
					AddBehavior(() => Position = character.img.Position);
					break;

				case WorldNameDB.FRENZY:
					AddBehavior(() =>
					{
						Position = character.head.Position;
						character.GetParent().MoveChild(this, 0);
					});
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

				case WorldNameDB.INTIMIDATING_SHOUT:
					AddBehavior(() => Position = character.head.Position + new Vector2(0.0f, 6.0f));

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
					AddBehavior(() => Position = character.head.Position);
					break;

				case WorldNameDB.SLOW:
					AddBehavior(() =>
					{
						// GetParent().RemoveChild(this);
						// bolt.target.AddChild(this);
						// Position = bolt.target.img.Position;
						// spell.Connect(nameof(Unmake), this, nameof(_OnTimerTimeout));
						// tween.Start();
						// timer.Start();
					});
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

				case WorldNameDB.ARCANE_BOLT:
				case WorldNameDB.FIREBALL:
				case WorldNameDB.FROST_BOLT:
				case WorldNameDB.METEOR:
				case WorldNameDB.SHADOW_BOLT:
				case WorldNameDB.SIPHON_MANA:
					AddBehavior(() =>
					{
						Node2D bolt = idleParticles.GetNode<Node2D>("bolt");
						tween.InterpolateProperty(bolt, ":modulate", bolt.Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.In);
					});
					break;

				case WorldNameDB.CONCUSSIVE_SHOT:
					// polyBehavior.Push(() =>
					// 	{
					// 		light.Show();
					// 		PackedScene bashScene = (PackedScene)GD.Load("res://src/spell/effects/bash.tscn");
					// 		bash_effect bash = (bash_effect)bashScene.Instance();
					// 		character.target.AddChild(bash);
					// 		bash.Owner = character.target;
					// 		spell.Connect(nameof(Unmake), bash, nameof(bash_effect._OnTimerTimeout));
					// 		bash.OnHit();
					// 	});
					break;

				case WorldNameDB.EXPLOSIVE_ARROW:
				case WorldNameDB.SNIPER_SHOT:
					AddBehavior(() => light.Show());
					break;
			}
		}
		private void AddBehavior(Routine routine) { polyBehavior.Push(routine); }
		public virtual void _OnTimerTimeout() { onTimeOut?.Invoke(); }
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
		public virtual void OnHit()
		{
			Globals.PlaySound(sound, this, new Utils.Speaker2D());

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

			while (polyBehavior.Count > 0)
			{
				polyBehavior.Pop().Invoke();
			}
		}
	}
}