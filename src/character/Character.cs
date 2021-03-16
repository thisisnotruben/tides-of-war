using Game.Database;
using Game.Actor.Doodads;
using Game.Actor.State;
using Game.Actor.Stat;
using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
namespace Game.Actor
{
	public abstract class Character : WorldObject, ISerializable
	{
		public const uint COLL_MASK_PLAYER = 0b_00000_00000_00000_10000,
			COLL_MASK_NPC = 0b_00000_00000_00000_00010,
			COLL_MASK_DEAD = 0b_00000_00000_00000_00100;

		public FSM fsm;
		public CombatTextHandler combatTextHandler;
		public StatManager stats;
		public Timer regenTimer;
		public Sprite img;
		public Position2D head, missileSpawnPos;
		public AnimationPlayer anim;
		public Area2D hitBox, sight;
		public AudioStreamPlayer2D player2D { get; protected set; }

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
				if (hp > stats.hpMax.valueI)
				{
					_hp = stats.hpMax.valueI;
				}
				else if (hp <= 0)
				{
					_hp = 0;
					mana = 0;
					state = FSM.State.DEAD;
				}
				else if (hp > 0 && dead)
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
				if (mana > stats.manaMax.valueI)
				{
					_mana = stats.manaMax.valueI;
				}
				else if (mana < 0)
				{
					_mana = 0;
				}
				EmitSignal(nameof(UpdateHudManaStatus), mana, stats.manaMax.valueI);
			}
		}

		public Character target;
		public readonly HashSet<ulong> pursuantUnitIds = new HashSet<ulong>();

		[Signal] public delegate void UpdateHudHealthStatus(int currentValue, int maxValue);
		[Signal] public delegate void UpdateHudManaStatus(int currentValue, int maxValue);
		[Signal] public delegate void NotifyAttack(Character whosAttacking);

		public override void _Ready()
		{
			regenTimer = GetNode<Timer>("regenTimer");
			anim = GetNode<AnimationPlayer>("anim");
			img = GetNode<Sprite>("img");
			head = GetNode<Position2D>("head");
			missileSpawnPos = img.GetNode<Position2D>("missile");
			combatTextHandler = img.GetNode<CombatTextHandler>("CombatTextHandler");
			player2D = img.GetNode<AudioStreamPlayer2D>("snd");
			fsm = GetNode<FSM>("fsm");
			hitBox = GetNode<Area2D>("area");
			sight = GetNode<Area2D>("sight");
			stats = new StatManager(this);

			fsm.Init(this);

			worldName = Name;
			// always starting with full health on init
			hp = stats.hpMax.valueI;
			mana = stats.manaMax.valueI;

			AddToGroup(Globals.SAVE_GROUP);
		}
		protected virtual void SetImg(string imgName)
		{
			ImageDB.ImageData imageData = Globals.imageDB.GetData(imgName);

			// set sprite
			Texture imgTexture = GD.Load<Texture>($"res://asset/img/character/{imgName}.png");
			img.Texture = imgTexture;
			img.Hframes = imageData.total;
			img.Position = new Vector2(0.0f, -imgTexture.GetHeight() / 2.0f);

			// set multiplier
			stats.multiplier = Stats.GetMultiplier(
				Globals.unitDB.HasData(Name)
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
			Texture hitBoxTexture = GD.Load<Texture>($"res://asset/img/character/resource/bodies/body-{imageData.body}.tres");
			TouchScreenButton select = GetNode<TouchScreenButton>("select");
			Node2D sightDistance = sight.GetNode<Node2D>("distance"),
				areaBody = hitBox.GetNode<Node2D>("body");

			select.Normal = hitBoxTexture;
			head.Position = new Vector2(0.0f, -hitBoxTexture.GetHeight());
			areaBody.Position = new Vector2(0.0f, -hitBoxTexture.GetHeight() / 2.0f);
			select.Position = new Vector2(-hitBoxTexture.GetWidth() / 2.0f, -hitBoxTexture.GetHeight());
			sightDistance.Position = areaBody.Position;
		}
		public void Harm(int damage, Vector2 direction) { fsm.Harm(damage, direction); }
		public void OnAttacked(Character whosAttacking) { fsm.OnAttacked(whosAttacking); }
		public void SpawnCombatText(string text, CombatText.TextType textType)
		{
			CombatText combatText = (CombatText)SceneDB.combatText.Instance();
			combatTextHandler.AddChild(combatText);
			combatText.Init(text, textType, img.Position);
			combatTextHandler.AddCombatText(combatText);
		}
		public void RemovePursuantUnitId(Character character)
		{
			pursuantUnitIds.Remove(character.GetInstanceId());
			if (pursuantUnitIds.Count == 0)
			{
				regenTimer.Start();
			}
		}
		public void _OnRegenTimerTimeout()
		{
			hp += stats.regenAmount.valueI;
			mana += stats.regenAmount.valueI;
		}
		public void _OnFootStep(bool rightStep)
		{
			// called from 'moving' animation
			if (!dead)
			{
				FootStep footStep = (FootStep)SceneDB.footStep.Instance();
				Vector2 stepPos = GlobalPosition;
				stepPos.y -= 3;
				stepPos.x += (rightStep) ? 1.0f : -4.0f;
				Map.Map.map.AddGChild(footStep);
				footStep.GlobalPosition = stepPos;
			}
		}
		public virtual GC.Dictionary Serialize()
		{
			GC.Dictionary payload = new GC.Dictionary()
			{
				{NameDB.SaveTag.POSITION, new GC.Array(){GlobalPosition.x, GlobalPosition.y}},
				{NameDB.SaveTag.TIME_LEFT, regenTimer.TimeLeft}
			};

			if (hp < stats.hpMax.valueI)
			{
				payload[NameDB.SaveTag.HP] = hp;
			}
			if (mana < stats.manaMax.valueI)
			{
				payload[NameDB.SaveTag.MANA] = mana;
			}
			if (target != null)
			{
				payload[NameDB.SaveTag.TARGET] = target.Name;
			}
			if (fsm.ShouldSerialize())
			{
				payload[NameDB.SaveTag.STATE] = fsm.Serialize();
			}

			return payload;
		}
		public virtual void Deserialize(GC.Dictionary payload)
		{
			foreach (string key in payload.Keys)
			{
				switch (key)
				{
					case NameDB.SaveTag.POSITION:
						GC.Array pos = (GC.Array)payload[key];
						GlobalPosition = new Vector2((float)pos[0], (float)pos[1]);
						break;

					case NameDB.SaveTag.HP:
						hp = payload[key].ToString().ToInt();
						break;

					case NameDB.SaveTag.MANA:
						mana = payload[key].ToString().ToInt();
						break;

					case NameDB.SaveTag.TIME_LEFT:
						float timeLeft = (float)payload[key];
						if (timeLeft > 0.0f)
						{
							regenTimer.WaitTime = timeLeft;
						}
						break;

					case NameDB.SaveTag.TARGET:
						string targetName = payload[key].ToString();
						if (GetParent().HasNode(targetName))
						{
							target = GetParent().GetNode<Character>(targetName);
						}
						break;
				}
			}

			string k = NameDB.SaveTag.STATE;
			if (payload.Contains(k))
			{
				fsm.Deserialize((GC.Dictionary)payload[k]);
			}
		}
	}
}