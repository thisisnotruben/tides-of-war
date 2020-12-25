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
		}
		public void Init(string worldName, Character exludedCharacter)
		{
			LandMineDB.LandMineData landMineData = Globals.landMineDB.GetData(worldName);

			this.worldName = worldName;
			this.exludedCharacter = exludedCharacter;
			this.timeToDetonationSec = landMineData.timeToDetSec;
			this.minDamage = landMineData.minDamage;
			this.maxDamage = landMineData.maxDamage;
			soundName = landMineData.sound;

			Map.Map.map.AddZChild(this);
			GlobalPosition = Map.Map.map.GetGridPosition(exludedCharacter.GlobalPosition);
			Owner = Map.Map.map;

			// activates detection
			body.Monitorable = true;
			body.Monitoring = true;

			if (landMineData.armDelaySec > 0.0f)
			{
				arming = true;
				timer.WaitTime = landMineData.armDelaySec;
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

			Globals.soundPlayer.PlaySound(soundName, player2D);

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
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.NAME, worldName},
				{NameDB.SaveTag.CHARACTER, exludedCharacter?.GetPath() ?? string.Empty},
				{NameDB.SaveTag.ARMING, arming},
				{NameDB.SaveTag.HIT, exploded},
				{NameDB.SaveTag.TIME_LEFT, timer.TimeLeft},
				{NameDB.SaveTag.POSITION, new GC.Array<float>() {GlobalPosition.x, GlobalPosition.y}}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			// TODO
			GC.Array<float> posArray = (GC.Array<float>)payload[NameDB.SaveTag.POSITION];
			GlobalPosition = new Vector2(posArray[0], posArray[1]);

			arming = (bool)payload[NameDB.SaveTag.ARMING];
			exploded = (bool)payload[NameDB.SaveTag.HIT];
			timer.WaitTime = (float)payload[NameDB.SaveTag.TIME_LEFT];
		}
	}
}