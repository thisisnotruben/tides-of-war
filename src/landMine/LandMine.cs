using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Godot;
using GC = Godot.Collections;
using System;
namespace Game.Mine
{
	public class LandMine : WorldObject, ICombustible, ISerializable
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
		private string soundName = string.Empty;

		public override void _Ready()
		{
			timer = GetNode<Timer>("timer");
			tween = GetNode<Tween>("tween");
			img = GetNode<Node2D>("img");
			explodeParticles = img.GetNode<Node2D>("explode");
			blastCircle = img.GetNode<Area2D>("blastCircle");
			body = img.GetNode<Area2D>("body");
			player2D = img.GetNode<AudioStreamPlayer2D>("snd");

			AddToGroup(Globals.SAVE_GROUP);
		}
		public LandMine Init(string worldName, Character exludedCharacter, bool arm = true)
		{
			LandMineDB.LandMineData landMineData = Globals.landMineDB.GetData(worldName);

			this.worldName = worldName;
			this.exludedCharacter = exludedCharacter;
			this.timeToDetonationSec = landMineData.timeToDetSec;
			this.minDamage = landMineData.minDamage;
			this.maxDamage = landMineData.maxDamage;
			soundName = landMineData.sound;

			Map.Map.map.AddZChild(this);
			Owner = Map.Map.map;
			GlobalPosition = Map.Map.map.GetGridPosition(exludedCharacter.GlobalPosition);

			if (arm)
			{
				ArmLandMine(landMineData);
			}
			return this;
		}
		private void ArmLandMine(LandMineDB.LandMineData landMineData)
		{
			CallDeferred(nameof(DeferredSetDetection));

			if (landMineData.armDelaySec > 0.0f)
			{
				arming = true;
				timer.Start(landMineData.armDelaySec);
			}
			else if (timeToDetonationSec > 0.0f)
			{
				timer.Start(timeToDetonationSec);
			}
		}
		private void DeferredSetDetection() { body.Monitoring = blastCircle.Monitoring = true; }
		public void OnTimerTimeout()
		{
			if (arming)
			{
				arming = false;

				if (timeToDetonationSec > 0.0f)
				{
					timer.Start(timeToDetonationSec);
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

			Globals.audioPlayer.PlaySound(soundName, player2D);

			// show vfx
			Particles2D particles2D;
			foreach (Node node in explodeParticles.GetChildren())
			{
				particles2D = node as Particles2D;
				if (particles2D != null)
				{
					particles2D.Emitting = true;
				}
			}

			// hit things around landMine
			Character character;
			foreach (Area2D area2D in blastCircle.GetOverlappingAreas())
			{
				character = area2D.Owner as Character;

				if (character != null && character != exludedCharacter)
				{
					character.Harm(damage, GlobalPosition);
					character.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
				}

				(area2D.Owner as ICombustible)?.Explode();
			}

			// fade out to delete
			tween.InterpolateProperty(img, "modulate",
				img.Modulate, Color.ColorN("white", 0.0f),
				0.5f, Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.InterpolateProperty(img, "scale",
				img.Scale, new Vector2(0.8f, 0.8f),
				0.5f, Tween.TransitionType.Bounce, Tween.EaseType.In);
			tween.Start();

			// makes sure to play effects/sounds before deleting
			timer.Start(5.0f);
		}
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.CHARACTER, (exludedCharacter?.GetPath() ?? string.Empty).ToString()},
				{NameDB.SaveTag.NAME, worldName},
				{NameDB.SaveTag.POSITION, new GC.Array() {GlobalPosition.x, GlobalPosition.y}},
				{NameDB.SaveTag.ARMING, arming},
				{NameDB.SaveTag.HIT, exploded},
				{NameDB.SaveTag.TIME_LEFT, timer.TimeLeft}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			GC.Array posArray = (GC.Array)payload[NameDB.SaveTag.POSITION];
			GlobalPosition = new Vector2((float)posArray[0], (float)posArray[1]);

			CallDeferred(nameof(DeferredSetDetection));

			arming = (bool)payload[NameDB.SaveTag.ARMING];
			exploded = (bool)payload[NameDB.SaveTag.HIT];

			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT];
			if (timeLeft > 0.0f)
			{
				timer.Start(timeLeft);
			}
			else
			{
				OnTimerTimeout();
			}
		}
	}
}