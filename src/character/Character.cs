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
		public static readonly PackedScene footStepScene = (PackedScene)GD.Load("res://src/character/doodads/footstep.tscn");
		public static readonly PackedScene buffAnimScene = (PackedScene)GD.Load("res://src/character/doodads/buff_anim.tscn");

		private protected FSM fsm;
		public StatManager stats;
		public Timer regenTimer;
		public Sprite img;
		public AnimationPlayer anim;

		public bool enemy { get; private protected set; }
		public bool dead { get { return fsm.IsDead(); } }
		public FSM.State state { get { return fsm.GetState(); } set { fsm.ChangeState(value); } }
		public Vector2 pos { get { return img.GlobalPosition; } }

		private int _level = Stats.MIN_LEVEL;
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
		private int _hp;
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
					state = FSM.State.DEAD;
				}
				else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				if (hp > 0 && fsm.IsDead())
				{
					state = FSM.State.ALIVE;
				}
				EmitSignal(nameof(UpdateHudStatus), this, true, hp, stats.hpMax.valueI);
			}
		}
		private int _mana;
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
				EmitSignal(nameof(UpdateHudStatus), this, false, mana, stats.manaMax.valueI);
			}
		}

		public Spell spell { get; private protected set; }
		public Character target;
		public List<Spell> spellQueue = new List<Spell>();
		public Dictionary<string, List<Item>> buffs = new Dictionary<string, List<Item>>();
		[Signal]
		public delegate void UpdateHudStatus(Character character, bool hp, int currentValue, int maxValue);
		[Signal]
		public delegate void UpdateHudIcon(string worldName, Pickable pickable, float seek);

		public override void _Ready()
		{
			regenTimer = GetNode<Timer>("regenTimer");
			anim = GetNode<AnimationPlayer>("anim");
			img = GetNode<Sprite>("img");
			fsm = GetNode<FSM>("fsm");
			stats = new StatManager(this);

			// always starting with full health on init
			hp = stats.hpMax.valueI;
			mana = stats.manaMax.valueI;
		}
		public virtual void Init()
		{
			buffs.Add("active", new List<Item>());
			buffs.Add("pending", new List<Item>());
			worldName = Name;
			spell = null;
			target = null;
		}
		private protected void SetImg(string imgName)
		{
			ImageDB.ImageNode imageData = ImageDB.GetImageData(imgName);

			// set sprite
			img.Texture = (Texture)GD.Load($"res://asset/img/character/{imgName}.png");
			img.Hframes = imageData.total;

			// set multiplier
			stats.multiplier = Stats.GetMultiplier(
				(UnitDB.HasUnitData(Name))
				? imgName.Split('-')[0] // character race
				: Name); // is player
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

			// set unit weapon range based on sprite
			stats.weaponRange.baseValue = (imageData.melee) ? Stats.WEAPON_RANGE_MELEE : Stats.WEAPON_RANGE_RANGE;

			// center nodes based sprite
			AtlasTexture texture = (AtlasTexture)GD.Load($"res://asset/img/character/resource/bodies/body-{imageData.body}.tres");
			TouchScreenButton select = GetNode<TouchScreenButton>("select");
			Vector2 textureSize = texture.Region.Size;
			Node2D sightDistance = GetNode<Node2D>("sight/distance");
			Node2D areaBody = GetNode<Node2D>("area/body");
			Node2D head = GetNode<Node2D>("head");

			select.Normal = texture;
			img.Position = new Vector2(0.0f, -img.Texture.GetHeight() / 2.0f);
			head.Position = new Vector2(0.0f, -texture.GetHeight());
			select.Position = new Vector2(-textureSize.x / 2.0f, -textureSize.y);
			areaBody.Position = new Vector2(-0.5f, -textureSize.y / 2.0f);
			sightDistance.Position = areaBody.Position;

		}
		public override void _Process(float delta) { fsm.Process(delta); }
		public void Harm(int damage) { fsm.Harm(damage); }
		public void SpawnCombatText(string text, CombatText.TextType textType)
		{
			CombatText combatText = (CombatText)Globals.combatText.Instance();
			AddChild(combatText);
			combatText.SetType(text, textType, GetNode<Node2D>("img").Position);
		}
		public async void Cast()
		{
			if (spell != null)
			{
				SetProcess(false);
				spell.Cast();
				await ToSignal(anim, "animation_finished");
				img.Frame = 0;
				SetProcess(true);
			}
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
		public void SetBuff(List<Item> buffPool = null, float seek = 0.0f)
		{
			if (buffPool == null)
			{
				buffPool = buffs["pending"];
			}
			foreach (Item buff in buffPool)
			{
				if (seek == 0.0f)
				{
					BuffAnim buffAnim = (BuffAnim)buffAnimScene.Instance();
					GetNode("img").AddChild(buffAnim);
					buffAnim.item = buff;
					buff.Connect(nameof(Item.UnMake), buffAnim, nameof(QueueFree));
				}
				buff.GetNode<Timer>("timer").Start();
				foreach (Item currentBuff in buffs["active"])
				{
					if (currentBuff.subType == buff.subType && !buff.worldName.ToLower().Contains("potion"))
					{
						currentBuff.ConfigureBuff(this, true);
					}
				}
				if (buff.subType != Item.WorldTypes.HEALING && buff.subType != Item.WorldTypes.MANA)
				{
					EmitSignal(nameof(UpdateHudIcon), worldName, buff, seek);
				}
				buffs["active"].Remove(buff);
				buffPool.Remove(buff);
			}
		}
		public void _OnRegenTimerTimeout()
		{
			// fsm controls when regen timer is allowed
			hp += stats.regenAmount.valueI;
			mana += stats.regenAmount.valueI;
		}
		public void _OnFootStep(bool rightStep)
		{
			// called from 'moving' animation
			if (!fsm.IsDead())
			{
				FootStep footStep = (FootStep)footStepScene.Instance();
				Vector2 stepPos = GlobalPosition;
				stepPos.y -= 3;
				stepPos.x += (rightStep) ? 1.0f : -4.0f;
				Map.Map.map.GetNode("ground").AddChild(footStep);
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
			Godot.Collections.Dictionary payload = new Godot.Collections.Dictionary()
			{
				{"GlobalPosition", new Godot.Collections.Array(){GlobalPosition.x, GlobalPosition.y}}
			};
			return payload;
		}
	}
}