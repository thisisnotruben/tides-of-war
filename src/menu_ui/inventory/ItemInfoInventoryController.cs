using Game.Actor.Doodads;
using Game.GameItem;
using Game.Database;
using Game.Factory;
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

			switch (Globals.itemDB.GetData(commodityWorldName).type)
			{
				case ItemDB.ItemType.ARMOR:
				case ItemDB.ItemType.WEAPON:
					unequipBttn.Visible = (player.weapon?.worldName.Equals(commodityWorldName) ?? false)
						|| (player.vest?.worldName.Equals(commodityWorldName) ?? false);
					equipBttn.Visible = !unequipBttn.Visible;
					iconView.slotType = SlotController.SlotTypes.EQUIP;
					break;
				default:
					unequipBttn.Visible = equipBttn.Visible = false;
					iconView.slotType = SlotController.SlotTypes.NORMAL;
					break;
			}
		}
		private void EquipItem(bool on, string worldName)
		{
			Item item = new ItemFactory().Make(player, worldName), equippedItem;
			switch (Globals.itemDB.GetData(worldName).type)
			{
				case ItemDB.ItemType.WEAPON:
					equippedItem = player.weapon;
					player.weapon = on ? item : null;
					break;
				case ItemDB.ItemType.ARMOR:
					equippedItem = player.vest;
					player.vest = on ? item : null;
					break;
				default:
					return;
			}

			if (on)
			{
				RemoveFromInventory();
			}
			else
			{
				int modelIndex = inventoryModel.AddCommodity(equippedItem.worldName);
				if (modelIndex != -1 && selectedSlotIdx != -1)
				{
					slotGridController.DisplaySlot(selectedSlotIdx,
						modelIndex, equippedItem.worldName);
				}
			}

			EmitSignal(nameof(RefreshSlots));
			SaveLoadModel.dirty = true;
		}
		private void RemoveFromInventory()
		{
			if (inventoryModel.RemoveCommodity(slotGridController.GetSlotToModelIndex(selectedSlotIdx)))
			{
				slotGridController.ClearSlot(selectedSlotIdx);
			}
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
					player.img.AddChild(SceneDB.buffAnimScene.Instance<BuffAnim>().Init(commodityWorldName));
					break;
			}
			PlaySound(sndName);

			// eat up or drink up
			new ItemFactory().Make(player, commodityWorldName).Start();
			RemoveFromInventory();
			CheckHudSlots(inventoryModel, commodityWorldName);

			EmitSignal(nameof(RefreshSlots));
			Hide();
		}
		public void OnEquipPressed()
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
				Globals.audioPlayer.PlaySound(Globals.itemDB.GetData(commodityWorldName).material, false);
				EquipItem(true, commodityWorldName);
				Hide();
			}
		}
		public void OnUnequipPressed()
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

			popup.RouteConnection(nameof(OnDropConfirm), this);
			mainContent.Hide();
			popup.ShowConfirm("Drop?");
		}
		private void OnDropConfirm()
		{
			PlaySound(NameDB.UI.CLICK2);
			PlaySound(NameDB.UI.INVENTORY_DROP);

			RemoveFromInventory();

			CheckHudSlots(inventoryModel, commodityWorldName);
			EmitSignal(nameof(RefreshSlots));

			Map.Map.map.AddDrop(player.GlobalPosition, commodityWorldName);
			Globals.questMaster.CheckQuests(commodityWorldName,
				Globals.spellDB.HasData(commodityWorldName)
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				false
			);

			Hide();
		}
		public int GetSlotIndexFromHud(string itemName)
		{
			foreach (SlotController slot in slotGridController.GetSlots())
			{
				if (slot.commodityWorldName.Equals(itemName))
				{
					return slot.GetIndex();
				}
			}
			return -1;
		}
	}
}