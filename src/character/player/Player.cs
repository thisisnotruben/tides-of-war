using Game.Database;
using Game.Actor.Doodads;
using Game.Ui;
using Game.GameItem;
using Game.Factory;
using Godot;
using GC = Godot.Collections;
using System;
using System.Collections.Generic;
namespace Game.Actor
{
	public class Player : Character
	{
		public static Player player;

		public MenuMasterController menu { get; private set; }
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

			menu = GetNode<MenuMasterController>("menu");
			menu.ConnectPlayerToHud(this);
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
			GC.Dictionary inventory = new GC.Dictionary();
			InventoryModel inventoryModel = menu.gameMenu.playerInventory;
			for (int i = 0; i < inventoryModel.count; i++)
			{
				inventory[i] = new GC.Array()
				{
					 inventoryModel.GetCommodity(i),
					 inventoryModel.GetCommodityStack(i)
				};
			}
			if (inventory.Count > 0)
			{
				payload[NameDB.SaveTag.INVENTORY] = inventory;
				payload[NameDB.SaveTag.INVENTORY_SLOTS] = menu.inventorySlots.Serialize();
			}

			// spellBook
			GC.Array commodities = new GC.Array();
			menu.gameMenu.playerSpellBook.GetCommodities().ForEach(c => commodities.Add(c));
			if (commodities.Count > 0)
			{
				payload[NameDB.SaveTag.SPELL_BOOK] = commodities;
				payload[NameDB.SaveTag.SPELL_BOOK_SLOTS] = menu.spellSlots.Serialize();
			}

			// hudSlots
			GC.Dictionary hudSlots = new GC.Dictionary();
			SlotController slot;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				slot = node as SlotController;
				if (slot != null && !slot.IsAvailable())
				{
					hudSlots[node.Name] = slot.Serialize();
				}
			}
			if (hudSlots.Count > 0)
			{
				payload[NameDB.SaveTag.HUD_SLOTS] = hudSlots;
			}

			// weapon & armor
			if (weapon != null)
			{
				payload[NameDB.SaveTag.WEAPON] = weapon.worldName;
			}
			if (vest != null)
			{
				payload[NameDB.SaveTag.ARMOR] = vest.worldName;
			}

			// misc
			payload[NameDB.SaveTag.XP] = xp;
			payload[NameDB.SaveTag.GOLD] = gold;
			payload[NameDB.SaveTag.LEVEL] = level;

			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);

			ItemFactory itemFactory = new ItemFactory();

			string k;
			GC.Dictionary packet;
			foreach (string key in payload.Keys)
			{
				switch (key)
				{
					case NameDB.SaveTag.INVENTORY:
						packet = (GC.Dictionary)payload[key];
						List<string> indexes = new List<string>();
						foreach (string index in packet.Keys)
						{
							indexes.Add(index);
						}
						indexes.Sort();

						InventoryModel inventoryModel = menu.gameMenu.playerInventory;
						GC.Array package;
						indexes.ForEach(i =>
						{
							package = (GC.Array)packet[i];
							inventoryModel.PushCommodity(package[0].ToString(),
								package[1].ToString().ToInt());
						});

						k = NameDB.SaveTag.INVENTORY_SLOTS;
						if (payload.Contains(k))
						{
							menu.inventorySlots.Deserialize((GC.Dictionary)payload[k]);
						}
						break;

					case NameDB.SaveTag.SPELL_BOOK:
						foreach (string spellName in (GC.Array)payload[key])
						{
							menu.gameMenu.playerSpellBook.AddCommodity(spellName);
						}

						k = NameDB.SaveTag.SPELL_BOOK_SLOTS;
						if (payload.Contains(k))
						{
							menu.spellSlots.Deserialize((GC.Dictionary)payload[k]);
						}
						break;

					case NameDB.SaveTag.HUD_SLOTS:
						packet = (GC.Dictionary)payload[key];
						foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
						{
							if (packet.Contains(node.Name))
							{
								(node as ISerializable)?.Deserialize((GC.Dictionary)packet[node.Name]);
							}
						}
						break;

					case NameDB.SaveTag.WEAPON:
						string weaponName = payload[key].ToString();
						weapon = Globals.itemDB.HasData(weaponName)
							? itemFactory.Make(this, weaponName)
							: null;
						break;

					case NameDB.SaveTag.ARMOR:
						string armorName = payload[key].ToString();
						vest = Globals.itemDB.HasData(armorName)
							? itemFactory.Make(this, armorName)
							: null;
						break;

					case NameDB.SaveTag.XP:
						xp = payload[key].ToString().ToInt();
						break;

					case NameDB.SaveTag.GOLD:
						gold = payload[key].ToString().ToInt();
						break;

					case NameDB.SaveTag.LEVEL:
						level = payload[key].ToString().ToInt();
						break;
				}
			}
			menu.SetTargetDisplay(target);
		}
	}
}