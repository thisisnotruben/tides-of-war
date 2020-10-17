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
			sound = SpellDB.GetSpellData(spellWorldName).sound;

			attachTo.AddChild(this);
			Owner = attachTo;

			// default behavior
			AddBehavior(() =>
			{
				tween.Start();
				timer.Start();
			});

			// optional onTimeOut behavior
			Routine fadeLightRoutine = () =>
			{
				FadeLight();
				SetEmitting(idleParticles, false);
				timer.Start();
			};

			// added behavior
			switch (spellWorldName)
			{
				case WorldNameDB.BASH:
				case WorldNameDB.DIVINE_HEAL:
				case WorldNameDB.FORTIFY:
					AddBehavior(() => Position = character.head.Position);
					onTimeOut = () =>
					{
						tween.InterpolateProperty(this, ":modulate", Modulate,
							new Color("00ffffff"), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.InOut);
						tween.Start();
						fadeLightRoutine();
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
					onTimeOut = fadeLightRoutine;
					break;

				case WorldNameDB.INTIMIDATING_SHOUT:
					AddBehavior(() => Position = character.head.Position + new Vector2(0.0f, 6.0f));
					onTimeOut = fadeLightRoutine;
					break;

				case WorldNameDB.MIND_BLAST:
					AddBehavior(() => Position = character.head.Position);
					break;

				case WorldNameDB.SLOW:
					AddBehavior(() => Position = character.img.Position);
					onTimeOut = fadeLightRoutine;
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
					// remember, bash effect can change and this with it
					string bashName = WorldNameDB.BASH;
					SpellEffect bashEffect = SpellDB.GetSpellEffect(SpellDB.GetSpellData(bashName).spellEffect);
					bashEffect.Init(character, bashName, character);

					polyBehavior.Push(() =>
						{
							light.Show();
							bashEffect.OnHit();
						});
					break;

				case WorldNameDB.EXPLOSIVE_ARROW:
				case WorldNameDB.SNIPER_SHOT:
					AddBehavior(() => light.Show());
					break;
			}
		}
		private void AddBehavior(Routine routine) { polyBehavior.Push(routine); }
		public void FadeLight()
		{
			tween.InterpolateProperty(light, ":modulate", light.Modulate,
				new Color("00ffffff"), lightFadeDelay, Tween.TransitionType.Linear, Tween.EaseType.In);
		}
		public void _OnTimerTimeout() { onTimeOut?.Invoke(); }
		public void OnHit()
		{
			Globals.PlaySound(sound, this, new Utils.Speaker2D());

			tween.InterpolateProperty(this, ":scale", new Vector2(0.75f, 0.75f),
				Vector2.One, 0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);

			if (fadeLight)
			{
				FadeLight();
			}

			SetEmitting(idleParticles, false);
			SetEmitting(explodeParticles, true);

			while (polyBehavior.Count > 0)
			{
				polyBehavior.Pop().Invoke();
			}
		}
		private void SetEmitting(Node parent, bool emitting)
		{
			Particles2D particles;
			foreach (Node node in parent.GetChildren())
			{
				particles = node as Particles2D;
				if (particles != null)
				{
					particles.Emitting = emitting;
				}
			}
		}
	}
}