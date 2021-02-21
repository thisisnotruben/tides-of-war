using Game.Database;
using Game.Actor.Doodads;
using Game.Ui;
using Game.GameItem;
using Game.Factory;
using Godot;
using GC = Godot.Collections;
using System;
namespace Game.Actor
{
	public class Player : Character
	{
		public static Player player;

		public MenuHandlerController menu { get; private set; }
		public CharacterCamera camera { get; protected set; }
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

			camera = GetNode<CharacterCamera>("camera");
			SetImg("human-20");

			menu = GetNode<MenuHandlerController>("in_game_menu");
			menu.ConnectPlayerToHud(this);
			AddToGroup(Globals.SAVE_GROUP);
		}
		protected override void SetImg(string imgName)
		{
			base.SetImg(imgName);
			Vector2 cameraPos = hitBox.GetNode<Node2D>("body").Position;
			cameraPos.y = Math.Abs(cameraPos.y);
			camera.Position = cameraPos;
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
				CombatText combatText = (CombatText)SceneDB.combatText.Instance();
				AddChild(combatText);
				combatText.Init($"+{xp}", CombatText.TextType.XP, img.Position);
			}
			int _level = Stats.CheckLevel(xp);
			if (level != _level && level < Stats.MAX_LEVEL)
			{
				level = _level;
				if (!fromSaveFile)
				{
					Globals.soundPlayer.PlaySound(NameDB.UI.LEVEL_UP);
				}
				if (level > Stats.MAX_LEVEL)
				{
					level = Stats.MAX_LEVEL;
				}
			}
		}
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();

			// inventory
			GC.Array<string> commodities = new GC.Array<string>();
			menu.mainMenuController.playerInventory.GetCommodities().ForEach(c => commodities.Add(c));
			payload[NameDB.SaveTag.INVENTORY] = commodities;

			// spellBook
			commodities.Clear();
			menu.mainMenuController.playerSpellBook.GetCommodities().ForEach(c => commodities.Add(c));
			payload[NameDB.SaveTag.SPELL_BOOK] = commodities;

			// weapon & armor
			payload[NameDB.SaveTag.WEAPON] = weapon?.worldName ?? string.Empty;
			payload[NameDB.SaveTag.ARMOR] = vest?.worldName ?? string.Empty;

			// misc
			payload[NameDB.SaveTag.XP] = xp;
			payload[NameDB.SaveTag.GOLD] = gold;

			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);

			return; // TODO

			// inventory
			foreach (string itemName in (GC.Array)payload[NameDB.SaveTag.INVENTORY])
			{
				menu.mainMenuController.playerInventory.AddCommodity(itemName);
			}

			// spellBook
			foreach (string spellName in (GC.Array)payload[NameDB.SaveTag.SPELL_BOOK])
			{
				menu.mainMenuController.playerSpellBook.AddCommodity(spellName);
			}

			// weapon & armor
			ItemFactory itemFactory = new ItemFactory();
			string weaponName = (string)payload[NameDB.SaveTag.WEAPON],
				armorName = (string)payload[NameDB.SaveTag.ARMOR];

			if (!weaponName.Equals(string.Empty))
			{
				weapon = itemFactory.Make(this, weaponName);
			}
			if (!armorName.Equals(string.Empty))
			{
				vest = itemFactory.Make(this, armorName);
			}

			// misc
			xp = (int)(Single)payload[NameDB.SaveTag.XP];
			gold = (int)(Single)payload[NameDB.SaveTag.GOLD];
		}
	}
}