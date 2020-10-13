using System.Collections.Generic;
using System.Linq;
using Game.Ability;
using Game.Database;
using Game.Loot;
using Game.Actor.Doodads;
using Game.Actor.State;
using Game.Actor.Stat;
using Godot;
namespace Game.Actor
{
	public abstract class Character : WorldObject, ISerializable
	{
		public const uint COLL_MASK_PLAYER = 0b_00000_00000_00000_10000,
			COLL_MASK_NPC = 0b_00000_00000_00000_00010,
			COLL_MASK_DEAD = 0b_00000_00000_00000_00100;

		protected FSM fsm;
		public CombatTextHandler combatTextHandler;
		public StatManager stats;
		public Timer regenTimer;
		public Sprite img;
		public Position2D head, missileSpawnPos;
		public AnimationPlayer anim;
		public Area2D hitBox, sight;
		public Camera2D camera { get; protected set; }

		public bool enemy { get; protected set; }
		public bool dead { get { return fsm.IsDead(); } }
		public bool attacking { get { return fsm.IsAtacking(); } }
		public bool moving { get { return fsm.IsMoving(); } }
		public FSM.State state { get { return fsm.GetState(); } set { fsm.ChangeState(value); } }
		public Vector2 pos { get { return img.GlobalPosition; } }

		private int _level = Stats.MIN_LEVEL, _hp, _mana;
		public int level
		{
			get { return _level; }
			set
			{
				if (level != value && value >= Stats.MIN_LEVEL && value <= Stats.MAX_LEVEL)
				{
					_level = value;
					stats.Recalculate();
					// perks of leveling up!
					hp = stats.hpMax.valueI;
					mana = stats.manaMax.valueI;
				}
			}
		}
		public int hp
		{
			get { return _hp; }
			set
			{
				_hp = value;
				if (hp >= stats.hpMax.valueI)
				{
					_hp = stats.hpMax.valueI;
					if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
					{
						RemoveFromGroup(Globals.SAVE_GROUP);
					}
				}
				else if (hp <= 0)
				{
					_hp = 0;
					mana = 0;
					state = FSM.State.DEAD;
				}
				else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				if (hp > 0 && dead)
				{
					state = FSM.State.ALIVE;
				}
				EmitSignal(nameof(UpdateHudHealthStatus), hp, stats.hpMax.valueI);
			}
		}
		public int mana
		{
			get { return _mana; }
			set
			{
				_mana = value;
				if (mana >= stats.manaMax.valueI)
				{
					_mana = stats.manaMax.valueI;
					if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
					{
						RemoveFromGroup(Globals.SAVE_GROUP);
					}
				}
				else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				if (mana < 0)
				{
					_mana = 0;
				}
				EmitSignal(nameof(UpdateHudManaStatus), mana, stats.manaMax.valueI);
			}
		}

		public Spell spell { get; private protected set; }
		public Character target;
		public List<Spell> spellQueue = new List<Spell>();

		[Signal] public delegate void UpdateHudHealthStatus(int currentValue, int maxValue);
		[Signal] public delegate void UpdateHudManaStatus(int currentValue, int maxValue);
		[Signal] public delegate void UpdateHudIcon(string worldName, Pickable pickable, float seek);
		[Signal] public delegate void NotifyAttack(Character whosAttacking);

		public override void _Ready()
		{
			regenTimer = GetNode<Timer>("regenTimer");
			anim = GetNode<AnimationPlayer>("anim");
			img = GetNode<Sprite>("img");
			head = GetNode<Position2D>("head");
			missileSpawnPos = img.GetNode<Position2D>("missile");
			combatTextHandler = img.GetNode<CombatTextHandler>("CombatTextHandler");
			camera = GetNode<Camera2D>("camera");
			fsm = GetNode<FSM>("fsm");
			hitBox = GetNode<Area2D>("area");
			sight = GetNode<Area2D>("sight");
			stats = new StatManager(this);

			fsm.Init(this);

			worldName = Name;
			// always starting with full health on init
			hp = stats.hpMax.valueI;
			mana = stats.manaMax.valueI;
		}
		protected void SetImg(string imgName)
		{
			ImageDB.ImageNode imageData = ImageDB.GetImageData(imgName);

			// set sprite
			Texture imgTexture = (Texture)GD.Load($"res://asset/img/character/{imgName}.png");
			img.Texture = imgTexture;
			img.Hframes = imageData.total;
			img.Position = new Vector2(0.0f, -imgTexture.GetHeight() / 2.0f);

			// set multiplier
			stats.multiplier = Stats.GetMultiplier(
				UnitDB.HasUnitData(Name)
				? imgName.Split('-')[0] // character race
				: Name); // is player

			// set unit weapon range based on sprite
			stats.weaponRange.baseValue = (imageData.melee) ? Stats.WEAPON_RANGE_MELEE : Stats.WEAPON_RANGE_RANGE;
			stats.Recalculate();

			// set sprite animation key-frames
			Animation animRes = anim.GetAnimation("attacking");
			animRes.TrackSetKeyValue(0, 0, imageData.moving + imageData.dying);
			animRes.TrackSetKeyValue(0, 1, imageData.total - 1);
			animRes = anim.GetAnimation("moving");
			animRes.TrackSetKeyValue(0, 0, 0);
			animRes.TrackSetKeyValue(0, 1, imageData.moving);
			animRes = anim.GetAnimation("dying");
			animRes.TrackSetKeyValue(0, 0, imageData.moving);
			animRes.TrackSetKeyValue(0, 1, imageData.dying);
			animRes = anim.GetAnimation("casting");
			animRes.TrackSetKeyValue(0, 0, imageData.moving);
			animRes.TrackSetKeyValue(0, 1, imageData.moving + 3);

			// center nodes based sprite
			Texture hitBoxTexture = (Texture)GD.Load($"res://asset/img/character/resource/bodies/body-{imageData.body}.tres");
			TouchScreenButton select = GetNode<TouchScreenButton>("select");
			Node2D sightDistance = sight.GetNode<Node2D>("distance"),
				areaBody = hitBox.GetNode<Node2D>("body");

			select.Normal = hitBoxTexture;
			head.Position = new Vector2(0.0f, -hitBoxTexture.GetHeight());
			areaBody.Position = new Vector2(0.0f, -hitBoxTexture.GetHeight() / 2.0f);
			camera.Position = new Vector2(0.0f, hitBoxTexture.GetHeight() / 2.0f);
			select.Position = new Vector2(-hitBoxTexture.GetWidth() / 2.0f, -hitBoxTexture.GetHeight());
			sightDistance.Position = areaBody.Position;
		}
		public void Harm(int damage) { fsm.Harm(damage); }
		public void OnAttacked(Character whosAttacking) { fsm.OnAttacked(whosAttacking); }
		public void SpawnCombatText(string text, CombatText.TextType textType)
		{
			CombatText combatText = (CombatText)CombatText.scene.Instance();
			combatTextHandler.AddChild(combatText);
			combatText.Init(text, textType, img.Position);
			combatTextHandler.AddCombatText(combatText);
		}
		public void SetCurrentSpell(Spell spell) { this.spell = spell; }
		public void SetSpell(Spell spell, float seek = 0.0f)
		{
			foreach (Spell otherSpell in spellQueue)
			{
				if (otherSpell.Equals(spell))
				{
					otherSpell.UnMake();
				}
			}
			if (spell.duration > 0.0f)
			{
				spellQueue.Add(spell);
				spell.Connect(nameof(Spell.Unmake), this, nameof(RemoveFromSpellQueue), new Godot.Collections.Array() { spell });
			}
			EmitSignal(nameof(UpdateHudIcon), worldName, spell, seek);
		}
		public void RemoveFromSpellQueue(Spell spell) { spellQueue.Remove(spell); }
		public void _OnRegenTimerTimeout()
		{
			// fsm controls when regen timer is allowed
			hp += stats.regenAmount.valueI;
			mana += stats.regenAmount.valueI;
		}
		public void _OnFootStep(bool rightStep)
		{
			// called from 'moving' animation
			if (!dead)
			{
				FootStep footStep = (FootStep)FootStep.scene.Instance();
				Vector2 stepPos = GlobalPosition;
				stepPos.y -= 3;
				stepPos.x += (rightStep) ? 1.0f : -4.0f;
				Map.Map.map.AddGChild(footStep);
				footStep.GlobalPosition = stepPos;
			}
		}
		public virtual void Deserialize(Godot.Collections.Dictionary payload)
		{
			// set global position
			Godot.Collections.Array globalPositionArray = (Godot.Collections.Array)payload["GlobalPosition"];
			Vector2 globalPosition = new Vector2((float)globalPositionArray[0], (float)globalPositionArray[1]);
			GlobalPosition = Map.Map.map.GetGridPosition(globalPosition);
		}
		public virtual Godot.Collections.Dictionary Serialize()
		{
			return new Godot.Collections.Dictionary()
			{
				{"GlobalPosition", new Godot.Collections.Array(){GlobalPosition.x, GlobalPosition.y}}
			};
		}
	}
}