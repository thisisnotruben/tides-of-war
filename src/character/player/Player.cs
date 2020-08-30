using System;
using Game.Actor.Doodads;
using Game.Ui;
using Game.Utils;
using Game.Loot;
using Game.ItemPoto;
using Godot;
namespace Game.Actor
{
	public class Player : Character
	{
		public static Player player;

		public int xp { get; private set; }
		public int gold;
		private Commodity _weapon;
		public Commodity weapon
		{
			get { return _weapon; }
			set
			{
				if (weapon != null)
				{
					weapon.Exit();
				}
				if (value != null)
				{
					value.Start();
				}
				_weapon = value;
			}
		}
		private Commodity _vest;
		public Commodity vest
		{
			get { return _vest; }
			set
			{
				if (vest != null)
				{
					vest.Exit();
				}
				if (value != null)
				{
					value.Start();
				}
				_weapon = value;
			}
		}

		public Player() { player = this; }
		public override void _Ready()
		{
			base._Ready();
			GameMenu.player = this;
			gold = 10_000;
			level = Stats.MAX_LEVEL;
			SetImg("human-6");
			GetMenu().ConnectPlayerToHud(this);
		}
		public override void _UnhandledInput(InputEvent @event) { fsm.UnhandledInput(@event); }
		public void _OnAnimFinished(string animName)
		{
			if (animName.Equals("attacking") && spell != null)
			{
				// TODO: perhaps this came from the spell; configure this when you makr the state
				// weaponRange = (ranged) ? Stats.WEAPON_RANGE_RANGE : Stats.WEAPON_RANGE_MELEE;
			}
			else if (animName.Equals("casting"))
			{
				SetProcess(true);
			}
		}
		public void SetXP(int addedXP, bool showLabel = true, bool fromSaveFile = false)
		{
			xp += addedXP;
			if (xp > Stats.MAX_XP)
			{
				xp = Stats.MAX_XP;
			}
			else if (xp > 0 && xp < Stats.MAX_XP && showLabel)
			{
				CombatText combatText = (CombatText)Globals.combatText.Instance();
				AddChild(combatText);
				combatText.SetType($"+{xp}", CombatText.TextType.XP, GetNode<Node2D>("img").Position);
			}
			int _level = Stats.CheckLevel(xp);
			if (level != _level && level < Stats.MAX_LEVEL)
			{
				level = _level;
				if (!fromSaveFile)
				{
					Globals.PlaySound("level_up", this, new Speaker());
				}
				if (level > Stats.MAX_LEVEL)
				{
					level = Stats.MAX_LEVEL;
				}
			}
		}
		public MenuHandler GetMenu() { return GetNode<MenuHandler>("in_game_menu"); }
	}
}