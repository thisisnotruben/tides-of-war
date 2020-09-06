using Game.Actor.Doodads;
using Game.Ui;
using Game.Utils;
using Game.ItemPoto;
using Godot;
namespace Game.Actor
{
	public class Player : Character
	{
		public static Player player;

		public MenuHandler menu { get; private set; }
		public int xp { get; private set; }
		public int gold;

		private Item _weapon, _vest;
		public Item weapon
		{
			get { return _weapon; }
			set
			{
				weapon?.Exit();
				value?.Start();
				_weapon = value;
			}
		}
		public Item vest
		{
			get { return _vest; }
			set
			{
				vest?.Exit();
				value?.Start();
				_vest = value;
			}
		}

		public Player() { player = this; }
		public override void _Ready()
		{
			base._Ready();
			GameMenu.player = this;
			gold = 10_000;
			// level = Stats.MAX_LEVEL;
			SetImg("human-6");
			menu = GetNode<MenuHandler>("in_game_menu");
			menu.ConnectPlayerToHud(this);
		}
		public override void _UnhandledInput(InputEvent @event) { fsm.UnhandledInput(@event); }
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
	}
}