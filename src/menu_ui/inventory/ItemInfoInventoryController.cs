using Game.Actor.Doodads;
using Game.GameItem;
using Game.Database;
using Game.Factory;
using Game.Quest;
using Game.Loot;
using Godot;
namespace Game.Ui
{
	public class ItemInfoInventoryController : ItemInfoController
	{
		[Signal] public delegate void RefreshSlots();

		public override void _Ready()
		{
			base._Ready();
			useBttn.Connect("pressed", this, nameof(OnUsePressed));
			equipBttn.Connect("pressed", this, nameof(OnEquipPressed));
			unequipBttn.Connect("pressed", this, nameof(OnUnequipPressed));
			dropBttn.Connect("pressed", this, nameof(OnDropPressed));
		}
		public override void Display(string commodityWorldName, bool allowMove)
		{
			base.Display(commodityWorldName, allowMove);

			unequipBttn.Visible = (player.weapon?.worldName.Equals(commodityWorldName) ?? false)
				|| (player.vest?.worldName.Equals(commodityWorldName) ?? false);
			equipBttn.Visible = !unequipBttn.Visible;
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
						inventoryModel.AddCommodity(player.weapon.worldName);
					}

					player.weapon = on ? item : null;
					break;
				case ItemDB.ItemType.ARMOR:
					// add back to inventory
					if (!on && player.vest != null)
					{
						inventoryModel.AddCommodity(player.vest.worldName);
					}

					player.vest = on ? item : null;
					break;
				default:
					return;
			}
			if (on)
			{
				slotGridController.ClearSlot(inventoryModel.RemoveCommodity(worldName));
			}
			EmitSignal(nameof(RefreshSlots));
		}
		protected void OnUsePressed()
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
						popup.ShowError("Cannot Eat\nIn Combat!");
						return;
					}
					break;
				case ItemDB.ItemType.POTION:
					sndName = NameDB.UI.DRINK;
					player.img.AddChild(((BuffAnim)SceneDB.buffAnimScene.Instance()).Init(commodityWorldName));
					break;
			}
			PlaySound(sndName);

			// eat up or drink up
			new ItemFactory().Make(player, commodityWorldName).Start();

			// remove from inventory
			int modelIndex = inventoryModel.RemoveCommodity(commodityWorldName);
			if (modelIndex != -1)
			{
				slotGridController.ClearSlot(modelIndex);
			}

			CheckHudSlots(inventoryModel, commodityWorldName);

			EmitSignal(nameof(RefreshSlots));
			Hide();
		}
		private void OnEquipPressed()
		{
			ItemDB.ItemType itemType = Globals.itemDB.GetData(commodityWorldName).type;
			Item playerWeapon = player.weapon,
				playerArmor = player.vest;
			bool inventoryFull = false;

			// unequip weapon if any to equip focused weapon
			if (itemType == ItemDB.ItemType.WEAPON && playerWeapon != null)
			{
				inventoryFull = inventoryModel.IsFull(playerWeapon.worldName);
				if (!inventoryFull)
				{
					EquipItem(false, playerWeapon.worldName);
				}
			}

			// unequip armor if any to equip focused armor
			if (itemType == ItemDB.ItemType.ARMOR && playerArmor != null)
			{
				inventoryFull = inventoryModel.IsFull(playerArmor.worldName);
				if (!inventoryFull)
				{
					EquipItem(false, playerArmor.worldName);
				}
			}

			if (inventoryFull)
			{
				mainContent.Hide();
				popup.ShowError("Inventory\nFull!");
			}
			else
			{
				Globals.soundPlayer.PlaySound(Globals.itemDB.GetData(commodityWorldName).material, false);
				EquipItem(true, commodityWorldName);
				Hide();
			}
		}
		private void OnUnequipPressed()
		{
			if (inventoryModel.IsFull(commodityWorldName))
			{
				mainContent.Hide();
				popup.ShowError("Inventory\nFull!");
			}
			else
			{
				PlaySound(NameDB.UI.INVENTORY_UNEQUIP);
				EquipItem(false, commodityWorldName);
				Hide();
			}
		}
		private void OnDropPressed()
		{
			PlaySound(NameDB.UI.CLICK2);

			RouteConnections(nameof(OnDropConfirm));
			mainContent.Hide();
			popup.ShowConfirm("Drop?");
		}
		private void OnDropConfirm()
		{
			PlaySound(NameDB.UI.CLICK2);
			PlaySound(NameDB.UI.INVENTORY_DROP);

			EquipItem(false, commodityWorldName); // just in case if it's equipped

			// remove from inventory
			int modelIndex = inventoryModel.RemoveCommodity(commodityWorldName);
			if (modelIndex != -1)
			{
				slotGridController.ClearSlot(modelIndex);
			}

			CheckHudSlots(inventoryModel, commodityWorldName);
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