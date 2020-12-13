using Game.Loot;
using Game.GameItem;
using Game.Database;
using Game.Factory;
using Game.Quest;
using Godot;
namespace Game.Ui
{
	public class ItemInfoInventoryController : ItemInfoController
	{
		[Signal] public delegate void ItemEquipped(string worldName, bool on);
		[Signal] public delegate void RefreshSlots();

		public override void _Ready()
		{
			base._Ready();
			useBttn.Connect("pressed", this, nameof(_OnUsePressed));
			equipBttn.Connect("pressed", this, nameof(_OnEquipPressed));
			unequipBttn.Connect("pressed", this, nameof(_OnUnequipPressed));
			dropBttn.Connect("pressed", this, nameof(_OnDropPressed));
		}
		private void EquipItem(bool on, string worldName)
		{
			Item item = new ItemFactory().Make(player, worldName);
			switch (Globals.itemDB.GetData(worldName).type)
			{
				case ItemDB.ItemType.WEAPON:
					// add back to inventory
					if (!on && player.weapon != null)
					{
						itemList.AddCommodity(player.weapon.worldName);
					}

					player.weapon = on ? item : null;
					break;
				case ItemDB.ItemType.ARMOR:
					// add back to inventory
					if (!on && player.vest != null)
					{
						itemList.AddCommodity(player.vest.worldName);
					}

					player.vest = on ? item : null;
					break;
				default:
					return;
			}
			if (on)
			{
				itemList.RemoveCommodity(worldName);
			}
			else
			{
				itemList.AddCommodity(worldName);
			}
			EmitSignal(nameof(RefreshSlots));
			EmitSignal(nameof(ItemEquipped), worldName, on);
		}
		public void _OnUsePressed()
		{
			string sndName = NameDB.UI.CLICK2;

			switch (Globals.itemDB.GetData(commodityWorldName).type)
			{
				case ItemDB.ItemType.FOOD:
					sndName = NameDB.UI.EAT;
					// cannot eat in combat
					if (player.attacking)
					{
						mainContent.Hide();
						popupController.ShowError("Cannot Eat\nIn Combat!");
						return;
					}
					break;
				case ItemDB.ItemType.POTION:
					sndName = NameDB.UI.DRINK;
					break;
			}
			PlaySound(sndName);

			// eat up or drink up
			new ItemFactory().Make(player, commodityWorldName).Start();
			// remove from inventory
			itemList.RemoveCommodity(commodityWorldName);
			CheckHudSlots(itemList, commodityWorldName);

			EmitSignal(nameof(RefreshSlots));
			Hide();
		}
		public void _OnEquipPressed()
		{
			ItemDB.ItemType itemType = Globals.itemDB.GetData(commodityWorldName).type;
			Item playerWeapon = player.weapon,
				playerArmor = player.vest;
			bool inventoryFull = false;

			// unequip weapon if any to equip focused weapon
			if (itemType == ItemDB.ItemType.WEAPON && playerWeapon != null)
			{
				inventoryFull = itemList.IsFull(playerWeapon.worldName);
				if (!inventoryFull)
				{
					EquipItem(false, playerWeapon.worldName);
				}
			}

			// unequip armor if any to equip focused armor
			if (itemType == ItemDB.ItemType.ARMOR && playerArmor != null)
			{
				inventoryFull = itemList.IsFull(playerArmor.worldName);
				if (!inventoryFull)
				{
					EquipItem(false, playerArmor.worldName);
				}
			}

			if (inventoryFull)
			{
				mainContent.Hide();
				popupController.ShowError("Inventory\nFull!");
			}
			else
			{
				Globals.soundPlayer.PlaySound(Globals.itemDB.GetData(commodityWorldName).material, false);
				EquipItem(true, commodityWorldName);
				Hide();
			}
		}
		public void _OnUnequipPressed()
		{
			if (itemList.IsFull(commodityWorldName))
			{
				mainContent.Hide();
				popupController.ShowError("Inventory\nFull!");
			}
			else
			{
				PlaySound(NameDB.UI.INVENTORY_UNEQUIP);
				EquipItem(false, commodityWorldName);
			}
		}
		public void _OnDropPressed()
		{
			PlaySound(NameDB.UI.CLICK2);

			RouteConnections(nameof(_OnDropConfirm));
			mainContent.Hide();
			popupController.ShowConfirm("Drop?");
		}
		public void _OnDropConfirm()
		{
			PlaySound(NameDB.UI.CLICK2);
			PlaySound(NameDB.UI.INVENTORY_DROP);

			// remove from inventory
			itemList.RemoveCommodity(commodityWorldName);
			CheckHudSlots(itemList, commodityWorldName);
			EmitSignal(nameof(RefreshSlots));

			// instance treasure chest
			TreasureChest treasureChest = (TreasureChest)SceneDB.treasureChest.Instance();
			treasureChest.Init(commodityWorldName);

			// place treasure chest in map
			Map.Map map = Map.Map.map;
			map.AddZChild(treasureChest);
			treasureChest.Owner = map;
			treasureChest.GlobalPosition = map.SetGetPickableLoc(player.GlobalPosition, true);

			QuestMaster.CheckQuests(commodityWorldName,
				Globals.spellDB.HasData(commodityWorldName)
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				false
			);

			Hide();
		}
	}
}