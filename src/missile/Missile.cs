using System;
using System.Collections.Generic;
using Game.Ability;
using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Game.Utils;
using Godot;
namespace Game.Missile
{
	public class Bolt : WorldObject
	{
		Vector2 spawnPos;
		bool rotate;
		bool reverse;
		bool hasHitTarget;
		bool instantSpawn;
		WorldTypes weaponType;
		WorldTypes swingType;
		private Character _target;
		public Character target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value;
				spawnPos = originator.GetNode<Node2D>("img/missile").GlobalPosition;
				if (rotate)
				{
					LookAt(target.pos);
				}
				if (instantSpawn)
				{
					GlobalPosition = target.pos;
				}
				else
				{
					SetProcess(true);
				}
				Show();
			}
		}
		Character originator;
		public Spell spell { get; private set; }

		[Signal]
		public delegate void Hit(Spell spell);
		public override void _Ready()
		{
			SetProcess(false);
		}
		public override void _Process(float delta)
		{
			if (rotate && !hasHitTarget)
			{
				LookAt(target.pos);
			}
			else if (reverse)
			{
				Move(target, originator);
			}
			else
			{
				Move(originator, target);
			}
		}
		public void _OnProjectileAreaEntered(Area2D area2D)
		{
			if (!hasHitTarget && area2D.Owner == target)
			{
				EmitSignal(nameof(Hit), spell);
				ZIndex = 1;
				hasHitTarget = true;
				if (GetNode<Sprite>("img").Texture != null)
				{
					AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
					if (spell != null)
					{
						anim.Play("img_fade");
					}
					else
					{
						anim.Play("fade");
					}
				}
				if (!target.dead)
				{
					Attack();
				}
			}
		}
		public void _OnAnimFinished(string animName)
		{
			if (animName.Equals("fade"))
			{
				SetProcess(false);
				GetNode<Tween>("tween").RemoveAll();
				QueueFree();
			}
		}
		private void Move(Character fromUnit, Character toUnit)
		{
			Tween tween = GetNode<Tween>("tween");
			tween.InterpolateProperty(this, ":global_position", GlobalPosition, toUnit.pos,
				spawnPos.DistanceTo(toUnit.pos) / fromUnit.stats.weaponRange.value,
				Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
		}
		public virtual void SetUp(Character originator, Vector2 globalPosition, Spell spell)
		{
			this.spell = spell;
			spawnPos = globalPosition;
			this.originator = originator;
		}
		public void Fade()
		{
			GetNode<AnimationPlayer>("anim").Play("fade");
		}
		public void Attack(bool ignoreArmor = false, Dictionary<string, Dictionary<string, int>> attackTable = null)
		{
			if (attackTable == null)
			{
				// TODO
				// attackTable = Stats.attackTable;
			}
			GD.Randomize();
			int damage = (int)Math.Round(GD.RandRange(originator.stats.minDamage.valueI, originator.stats.maxDamage.valueI));
			uint diceRoll = GD.Randi() % 100 + 1;
			string weaponTypeName = Enum.GetName(typeof(WorldTypes), weaponType);
			string swingTypeName = Enum.GetName(typeof(WorldTypes), swingType).ToLower();
			long sndIdx = GD.Randi() % Globals.WEAPON_TYPE[weaponTypeName];
			bool PlaySound = false;
			string snd = $"{weaponTypeName.ToLower()}{sndIdx}";
			CombatText.TextType hitType;
			if (diceRoll <= attackTable["RANGED"]["HIT"])
			{
				hitType = CombatText.TextType.HIT;
			}
			else if (diceRoll <= attackTable["RANGED"]["CRITICAL"])
			{
				hitType = CombatText.TextType.CRITICAL;
				damage *= 2;
			}
			else if (diceRoll <= attackTable["RANGED"]["DODGE"])
			{
				hitType = CombatText.TextType.DODGE;
				damage = 0;
				snd = swingTypeName + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
			}
			else
			{
				hitType = CombatText.TextType.MISS;
				damage = 0;
				snd = swingTypeName + GD.Randi() % Globals.WEAPON_TYPE[nameof(swingType)];
			}
			target.Harm(damage);
			target.SpawnCombatText(damage.ToString(), hitType);
			if (PlaySound)
			{
				Globals.PlaySound(snd, this, GetNode<Speaker2D>("snd"));
			}
		}
		public void Make()
		{
			string texturePath = "res://asset/img/missile-spell/{0}.tres";
			string textureSize = "big";
			string raceName = originator.img.Texture.ResourcePath.GetBaseDir().GetFile();
			switch (raceName)
			{
				case "gnoll":
				case "goblin":
					GetNode<Sprite>("img").Offset = new Vector2(-5.5f, 0.0f);
					textureSize = "small";
					break;
			}
			if (spell == null)
			{
				texturePath = string.Format(texturePath, "arrow_{0}0");
			}
			else
			{
				SpellEffect spellEffect = PickableFactory.GetMakeSpellEffect(spell.worldName);
				Connect(nameof(Hit), spellEffect, nameof(SpellEffect.OnHit));
				if (spell.worldType != WorldTypes.SLOW)
				{
					spellEffect.Connect(nameof(SpellEffect.Unmake), this, nameof(Fade));
				}
				AddChild(spellEffect);
				spellEffect.Owner = this;
				switch (spell.worldType)
				{
					case WorldTypes.FIREBALL:
					case WorldTypes.SHADOW_BOLT:
					case WorldTypes.FROST_BOLT:
					case WorldTypes.MIND_BLAST:
					case WorldTypes.SLOW:
					case WorldTypes.SIPHON_MANA:
						GetNode<CollisionShape2D>("coll").Shape = (Shape2D)GD.Load("res://asset/img/missile-spell/SpellHitBox.tres");
						switch (spell.worldType)
						{
							case WorldTypes.MIND_BLAST:
							case WorldTypes.SLOW:
								instantSpawn = true;
								break;
							case WorldTypes.SIPHON_MANA:
								instantSpawn = true;
								reverse = true;
								break;
							case WorldTypes.FROST_BOLT:
								rotate = true;
								break;
						}
						break;
					case WorldTypes.PIERCING_SHOT:
						texturePath = string.Format(texturePath, "arrow_{0}1");
						break;
					case WorldTypes.CONCUSSIVE_SHOT:
						texturePath = string.Format(texturePath, "arrow_{0}2");
						break;
					case WorldTypes.EXPLOSIVE_ARROW:
						texturePath = string.Format(texturePath, "arrow_{0}3");
						break;
					default:
						if (spell.worldName.Contains("shot") ||
							spell.worldName.Contains("arrow") ||
							spell.worldType == WorldTypes.VOLLEY)
						{
							texturePath = string.Format(texturePath, "arrow_{0}0");
						}
						break;
				}
			}
			if (texturePath.Contains("{0}") && texturePath.Contains("arrow"))
			{
				texturePath = string.Format(texturePath, textureSize);
				rotate = true;
				weaponType = WorldTypes.ARROW_HIT_ARMOR;
				swingType = WorldTypes.ARROW_PASS;
				GetNode<Node2D>("coll").Position = new Vector2(-6.0f, 0.0f);
				GetNode<Sprite>("img").Texture = (Texture)GD.Load(texturePath);
			}
		}
	}
}