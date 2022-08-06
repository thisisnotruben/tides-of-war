using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Godot;
using GC = Godot.Collections;
namespace Game.Ability
{
	public class SpellEffect : WorldObject, ISerializable
	{
		[Export] protected float lightFadeDelay = 0.65f;
		[Export] protected bool fadeLight = true;
		[Export] protected bool playSound = false; // for missile to play sound?

		private Tween tween;
		private Timer timer;
		private Sprite light;
		private Node2D idleParticles, explodeParticles;
		private string sound;
		private AudioStreamPlayer2D player2D;
		private bool active;

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
			player2D = GetNode<AudioStreamPlayer2D>("snd");

			foreach (Node2D node2D in idleParticles.GetChildren())
			{
				node2D.UseParentMaterial = true;
			}
			foreach (Node2D node2D in explodeParticles.GetChildren())
			{
				node2D.UseParentMaterial = true;
			}
		}
		public SpellEffect Init(Character character, string spellWorldName)
		{
			return Init(character, spellWorldName, character);
		}
		public SpellEffect Init(Character character, string spellWorldName, Node attachTo)
		{
			worldName = spellWorldName;
			sound = Globals.spellDB.GetData(spellWorldName).sound;

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
				GetParticles(idleParticles).ForEach(p => p.Emitting = false);
				timer.Start();
			};

			// added behavior
			switch (spellWorldName)
			{
				case NameDB.Spell.BASH:
				case NameDB.Spell.DIVINE_HEAL:
				case NameDB.Spell.FORTIFY:
					AddBehavior(() => Position = character.head.Position);
					onTimeOut = () =>
					{
						tween.InterpolateProperty(this, "modulate", Modulate,
							Color.ColorN("white", 0.0f), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.InOut);
						tween.Start();
						fadeLightRoutine();
					};
					break;

				case NameDB.Spell.CLEAVE:
				case NameDB.Spell.DEVASTATE:
				case NameDB.Spell.HASTE:
				case NameDB.Spell.HEMORRHAGE:
				case NameDB.Spell.OVERPOWER:
					AddBehavior(() => Position = character.img.Position);
					break;

				case NameDB.Spell.FRENZY:
					AddBehavior(() =>
					{
						Position = character.head.Position;
						character.GetParent().MoveChild(this, 0);
					});
					onTimeOut = fadeLightRoutine;
					break;

				case NameDB.Spell.INTIMIDATING_SHOUT:
					AddBehavior(() => Position = character.head.Position + new Vector2(0.0f, 6.0f));
					onTimeOut = fadeLightRoutine;
					break;

				case NameDB.Spell.MIND_BLAST:
					AddBehavior(() => Position = character.head.Position);
					break;

				case NameDB.Spell.SLOW:
					AddBehavior(() => Position = character.img.Position);
					onTimeOut = fadeLightRoutine;
					break;

				case NameDB.Spell.ARCANE_BOLT:
				case NameDB.Spell.FIREBALL:
				case NameDB.Spell.FROST_BOLT:
				case NameDB.Spell.METEOR:
				case NameDB.Spell.SHADOW_BOLT:
				case NameDB.Spell.SIPHON_MANA:
					AddBehavior(() =>
					{
						Node2D bolt = idleParticles.GetNode<Node2D>("bolt");
						tween.InterpolateProperty(bolt, "modulate", bolt.Modulate,
							Color.ColorN("white", 0.0f), lightFadeDelay,
							Tween.TransitionType.Linear, Tween.EaseType.In);
					});
					break;

				case NameDB.Spell.CONCUSSIVE_SHOT:
					string bashName = NameDB.Spell.BASH;
					SpellEffect bashEffect = Globals.spellEffectDB.GetData(
						Globals.spellDB.GetData(bashName).spellEffect).Instance<SpellEffect>().Init(
						character, bashName);

					polyBehavior.Push(() =>
						{
							light.Show();
							bashEffect.OnHit();
						});
					break;

				case NameDB.Spell.EXPLOSIVE_ARROW:
				case NameDB.Spell.SNIPER_SHOT:
					AddBehavior(() => light.Show());
					break;
			}
			return this;
		}
		private void AddBehavior(Routine routine) { polyBehavior.Push(routine); }
		public void FadeLight()
		{
			tween.InterpolateProperty(light, "modulate", light.Modulate,
				Color.ColorN("white", 0.0f), lightFadeDelay, Tween.TransitionType.Linear, Tween.EaseType.In);
		}
		public void _OnTimerTimeout() { onTimeOut?.Invoke(); }
		public void OnHit()
		{
			if (active)
			{
				return;
			}

			active = true;
			Globals.audioPlayer.PlaySound(sound, player2D);

			tween.InterpolateProperty(this, "scale", new Vector2(0.75f, 0.75f),
				Vector2.One, 0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);

			if (fadeLight)
			{
				FadeLight();
			}

			GetParticles(idleParticles).ForEach(p => p.Emitting = false);
			GetParticles(explodeParticles).ForEach(p => p.Emitting = true);

			while (polyBehavior.Count > 0)
			{
				polyBehavior.Pop().Invoke();
			}
		}
		private List<Particles2D> GetParticles(Node parent)
		{
			List<Particles2D> particles2D = new List<Particles2D>();
			for (int i = 0; i < parent.GetChildCount(); i++)
			{
				if (GetChild(i) is Particles2D)
				{
					particles2D.Add(GetChild<Particles2D>(i));
				}
			}
			return particles2D;
		}
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{ NameDB.SaveTag.NAME, Filename.GetFile().BaseName() },
				{ NameDB.SaveTag.SPELL, worldName },
				{ NameDB.SaveTag.TIME_LEFT, timer.TimeLeft },
				{ NameDB.SaveTag.ANIM_POSITION, tween.Tell() }
		};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT],
				timeAdvanced = timer.WaitTime - timeLeft;

			if (timeAdvanced > 0.0f)
			{
				GetParticles(idleParticles).ForEach(p => p.Preprocess = timeAdvanced);
				GetParticles(explodeParticles).ForEach(p => p.Preprocess = timeAdvanced);
			}

			OnHit();

			timer.Start(timeLeft);
			tween.Seek((float)payload[NameDB.SaveTag.ANIM_POSITION]);
		}
	}
}