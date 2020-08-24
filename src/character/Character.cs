using System.Collections.Generic;
using System.Linq;
using Game.Ability;
using Game.Database;
using Game.Loot;
using Game.Actor.Doodads;
using Game.Actor.State;
using Game.Utils;
using Godot;
namespace Game.Actor
{
	public abstract class Character : WorldObject, ISerializable
	{
		public static readonly PackedScene footStepScene = (PackedScene)GD.Load("res://src/character/doodads/footstep.tscn");
		public static readonly PackedScene buffAnimScene = (PackedScene)GD.Load("res://src/character/doodads/buff_anim.tscn");
		private protected FSM fsm;
		public enum States { ALIVE, DEAD, MOVING, IDLE, ATTACKING }
		public virtual States state { get; private set; }
		public bool enemy { get; private protected set; }
		private protected bool engaging;
		public bool dead { get; private set; }
		private int _level;
		public int level
		{
			get
			{
				return _level;
			}
			set
			{
				_level = (value > Stats.MAX_LEVEL) ? Stats.MAX_LEVEL : value;
				SetAttributes();
			}
		}
		public int armor;
		private int _hp;
		public int hp
		{
			get
			{
				return _hp;
			}
			set
			{
				_hp += value;
				if (hp >= hpMax)
				{
					_hp = hpMax;
					if (this is Npc && !spellQueue.Any() && IsInGroup(Globals.SAVE_GROUP))
					{
						RemoveFromGroup(Globals.SAVE_GROUP);
					}
				}
				else if (hp <= 0)
				{
					_hp = 0;
					SetState(States.DEAD);
				}
				else if (this is Npc && !IsInGroup(Globals.SAVE_GROUP))
				{
					AddToGroup(Globals.SAVE_GROUP);
				}
				if (hp > 0 && dead)
				{
					SetState(States.ALIVE);
				}
				EmitSignal(nameof(UpdateHudStatus), this, true, hp, hpMax);
			}
		}
		public int hpMax;
		private int _mana;
		public int mana
		{
			get
			{
				return _mana;
			}
			set
			{
				_mana += value;
				if (mana >= manaMax)
				{
					_mana = manaMax;
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
				EmitSignal(nameof(UpdateHudStatus), this, false, mana, manaMax);
			}
		}
		public int manaMax;
		public int regenTime;
		public int minDamage;
		public int maxDamage;
		public int stamina { get; private set; }
		public int intellect { get; private set; }
		public int agility { get; private set; }
		public int weaponRange = Stats.WEAPON_RANGE_MELEE;
		public float animSpeed;
		public float weaponSpeed;
		public Spell spell { get; private protected set; }
		public virtual Character target { get; set; }
		private protected Speaker2D snd;
		public List<Spell> spellQueue;
		private protected Dictionary<Character, int> targetList;
		private Dictionary<Character, int> attackerTable;
		public Dictionary<string, List<Item>> buffs;
		[Signal]
		public delegate void UpdateHudStatus(Character character, bool hp, int currentValue, int maxValue);
		[Signal]
		public delegate void UpdateHudIcon(string worldName, Pickable pickable, float seek);
		public Timer regenTimer;

		public virtual void init()
		{
			spellQueue = new List<Spell>();
			targetList = new Dictionary<Character, int>();
			attackerTable = new Dictionary<Character, int>();
			buffs = new Dictionary<string, List<Item>>();
			buffs.Add("active", new List<Item>());
			buffs.Add("pending", new List<Item>());
			worldName = Name;
			enemy = false;
			dead = false;
			_level = 0;
			armor = 0;
			hp = 0;
			hpMax = 0;
			mana = 0;
			manaMax = 0;
			regenTime = 0;
			minDamage = 0;
			maxDamage = 0;
			stamina = 0;
			intellect = 0;
			agility = 0;
			weaponRange = Stats.WEAPON_RANGE_MELEE;
			animSpeed = 1.0f;
			weaponSpeed = 1.3f;
			spell = null;
			target = null;
		}
		private protected void SetImg(string imgName)
		{
			ImageDB.ImageNode imageData = ImageDB.GetImageData(imgName);
			Sprite img = GetNode<Sprite>("img");
			img.Texture = (Texture)GD.Load($"res://asset/img/character/{imgName}.png");
			img.Hframes = imageData.total;
			AnimationPlayer anim = GetNode<AnimationPlayer>("anim");
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
			if (imageData.attacking == 0)
			{
				anim.RemoveAnimation("attacking");
			}
			weaponRange = (imageData.melee) ? Stats.WEAPON_RANGE_MELEE : Stats.WEAPON_RANGE_RANGE;

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
		private protected void SetAttributes()
		{
			Stats.CharacterStatsNode characterStatsNode = Stats.UnitMake(level,
				Stats.GetMultiplier(this is Npc, GetNode<Sprite>("img").Texture.ResourcePath));
			stamina = characterStatsNode.stamina;
			intellect = characterStatsNode.intellect;
			hpMax = characterStatsNode.hpMax;
			manaMax = characterStatsNode.manaMax;
			maxDamage = characterStatsNode.maxDamage;
			minDamage = characterStatsNode.minDamage;
			regenTime = characterStatsNode.regenTime;
			armor = characterStatsNode.armor;
		}
		public override void _Ready()
		{
			fsm = GetNode<FSM>("fsm");
			regenTimer = GetNode<Timer>("regenTimer");
			snd = GetNode<Speaker2D>("snd");
			GetNode<RayCast2D>("ray").AddException(GetNode<Area2D>("area"));
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
				await ToSignal(GetNode<AnimationPlayer>("anim"), "animation_finished");
				GetNode<Sprite>("img").Frame = 0;
				SetProcess(true);
			}
		}
		public Vector2 GetCenterPos() { return GetNode<Node2D>("img").GlobalPosition; }
		public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
		{
			Tween tween = GetNode<Tween>("tween");
			if (obj is Node2D)
			{
				Node2D node2Obj = (Node2D)obj;
				if (!node2Obj.Scale.Equals(new Vector2(1.0f, 1.0f)))
				{
					// Reverts to original scale when unit is clicked on
					tween.InterpolateProperty(node2Obj, nodePath, node2Obj.Scale,
						new Vector2(1.0f, 1.0f), 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
					tween.Start();
				}
			}
			if (tween.PauseMode == PauseModeEnum.Process)
			{
				// Condition when merchant/trainer are selected to let their
				// animation run through it's course when game is paused
				tween.PauseMode = PauseModeEnum.Inherit;
			}
		}
		public virtual void _OnSelectPressed() { /* Override for Npc */ }
		public virtual void SetState(States state, bool overrule = false) { this.state = state; }
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
			int regenAmount = Stats.HpManaRegenAmount(level,
				Stats.GetMultiplier(this is Npc, GetNode<Sprite>("img").Texture.ResourcePath));
			hp = regenAmount;
			mana = regenAmount;
		}
		public void _OnFootStep(bool rightStep)
		{
			// called from walking animation
			if (fsm.GetState() != FSM.State.DEAD
			|| fsm.GetState() != FSM.State.PLAYER_DEAD_IDLE
			|| fsm.GetState() != FSM.State.PLAYER_DEAD_MOVE)
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
			Godot.Collections.Array globalPositionArray = (Godot.Collections.Array)payload[nameof(GlobalPosition)];
			Vector2 globalPosition = new Vector2((float)globalPositionArray[0], (float)globalPositionArray[1]);
			GlobalPosition = Map.Map.map.GetGridPosition(globalPosition);
		}
		public virtual Godot.Collections.Dictionary Serialize()
		{
			Godot.Collections.Dictionary payload = new Godot.Collections.Dictionary()
			{
				{nameof(GlobalPosition), new Godot.Collections.Array(){GlobalPosition.x, GlobalPosition.y}}
			};
			return payload;
		}
	}
}