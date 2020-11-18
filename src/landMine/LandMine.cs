using Game.Actor;
using Game.Actor.Doodads;
using Game.Sound;
using Godot;
using System;
namespace Game.Mine
{
	public class LandMine : Node2D, ICombustible
	{
		private Area2D blastCircle, body;
		private Timer timer;
		private Tween tween;
		private Node2D img, explodeParticles;
		private AudioStreamPlayer2D player2D;

		private Character exludedCharacter;
		private float timeToDetonationSec = 1.0f;
		private bool arming, exploded;
		private int minDamage, maxDamage;

		public override void _Ready()
		{
			timer = GetNode<Timer>("timer");
			tween = GetNode<Tween>("tween");
			img = GetNode<Node2D>("img");
			explodeParticles = img.GetNode<Node2D>("explode");
			blastCircle = img.GetNode<Area2D>("blastCircle");
			body = img.GetNode<Area2D>("body");
			player2D = img.GetNode<AudioStreamPlayer2D>("snd");
		}
		public void Init(Character exludedCharacter, int minDamage, int maxDamage, float armDelaySec = -1.0f, float timeToDetonationSec = -1.0f)
		{
			this.exludedCharacter = exludedCharacter;
			this.timeToDetonationSec = timeToDetonationSec;
			this.minDamage = minDamage;
			this.maxDamage = maxDamage;

			Map.Map.map.AddZChild(this);
			GlobalPosition = Map.Map.map.GetGridPosition(exludedCharacter.GlobalPosition);
			Owner = Map.Map.map;

			// activates detection
			body.Monitorable = true;
			body.Monitoring = true;

			if (armDelaySec > 0.0f)
			{
				arming = true;
				timer.WaitTime = armDelaySec;
				timer.Start();
			}
			else if (timeToDetonationSec > 0.0f)
			{
				timer.WaitTime = timeToDetonationSec;
				timer.Start();
			}
		}
		public void OnTimerTimeout()
		{
			if (arming)
			{
				arming = false;

				if (timeToDetonationSec > 0.0f)
				{
					timer.WaitTime = timeToDetonationSec;
					timer.Start();
				}
				else
				{
					Explode();
				}
			}
			else if (exploded)
			{
				QueueFree();
			}
			else
			{
				Explode();
			}
		}
		public void OnCharacterEnteredBlastCircle(Area2D area2d)
		{
			Character character = area2d.Owner as Character;

			if (character != null && character != exludedCharacter)
			{
				Explode();
			}
		}
		public void Explode()
		{
			if (exploded)
			{
				return;
			}

			exploded = true;
			int damage = new Random().Next(minDamage, maxDamage + 1);

			SoundPlayer.INSTANCE.PlaySound("TODO", player2D);

			Particles2D particles2D;
			foreach (Node node in explodeParticles.GetChildren())
			{
				particles2D = node as Particles2D;
				if (particles2D != null)
				{
					particles2D.Emitting = true;
				}
			}

			Character character;
			foreach (Area2D area2D in blastCircle.GetChildren())
			{
				character = area2D.Owner as Character;

				if (character != null && character != exludedCharacter)
				{
					character.Harm(damage);
					character.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
				}

				(area2D.Owner as ICombustible)?.Explode();
			}

			tween.InterpolateProperty(img, ":modulate",
				img.Modulate, new Color("00ffffff"),
				0.5f, Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.InterpolateProperty(img, ":scale",
				img.Scale, new Vector2(0.8f, 0.8f),
				0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
			tween.Start();

			// makes sure to play effects/sounds before deleting
			timer.Stop();
			timer.WaitTime = 5.0f;
			timer.Start();
		}
	}
}