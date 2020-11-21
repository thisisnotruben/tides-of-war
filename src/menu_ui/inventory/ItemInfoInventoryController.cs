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
			switch (ItemDB.Instance.GetData(worldName).type)
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

			switch (ItemDB.Instance.GetData(pickableWorldName).type)
			{
				case ItemDB.ItemType.FOOD:
					sndName = NameDB.UI.EAT;
					// cannot eat in combat
					if (player.attacking)
					{
						mainContent.Hide();
						popupController.errorLabel.Text = "Cannot Eat\nIn Combat!";
						popupController.errorView.Show();
						popupController.Show();
						return;
					}
					break;
				case ItemDB.ItemType.POTION:
					sndName = NameDB.UI.DRINK;
					break;
			}
			Globals.soundPlayer.PlaySound(sndName);

			// eat up or drink up
			new ItemFactory().Make(player, pickableWorldName).Start();
			// remove from inventory
			itemList.RemoveCommodity(pickableWorldName);

			EmitSignal(nameof(RefreshSlots));
			Hide();
		}
		public void _OnEquipPressed()
		{
			ItemDB.ItemType itemType = ItemDB.Instance.GetData(pickableWorldName).type;
			Item playerWeapon = player.weapon;
			Item playerArmor = player.vest;
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
				// inventory full
				mainContent.Hide();
				popupController.errorLabel.Text = "Inventory\nFull!";
				popupController.errorView.Show();
				popupController.Show();
			}
			else
			{
				Globals.soundPlayer.PlaySound(ItemDB.Instance.GetData(pickableWorldName).material, false);
				// equip focused item
				EquipItem(true, pickableWorldName);
			}
		}
		public void _OnUnequipPressed()
		{
			if (itemList.IsFull(pickableWorldName))
			{
				mainContent.Hide();
				popupController.errorLabel.Text = "Inventory\nFull!";
				popupController.errorView.Show();
				popupController.Show();
			}
			else
			{
				Globals.soundPlayer.PlaySound(NameDB.UI.INVENTORY_UNEQUIP);
				EquipItem(false, pickableWorldName);
			}
		}
		public void _OnDropPressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);

			RouteConnections(nameof(_OnDropConfirm));
			mainContent.Hide();
			popupController.yesNoLabel.Text = "Drop?";
			popupController.yesNoView.Show();
			popupController.Show();
		}
		public void _OnDropConfirm()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
			Globals.soundPlayer.PlaySound(NameDB.UI.INVENTORY_DROP);

			// remove from inventory
			itemList.RemoveCommodity(pickableWorldName);
			EmitSignal(nameof(RefreshSlots));

			// instance treasure chest
			TreasureChest treasureChest = (TreasureChest)SceneDB.treasureChest.Instance();
			treasureChest.Init(pickableWorldName);

			// place treasure chest in map
			Map.Map map = Map.Map.map;
			map.AddZChild(treasureChest);
			treasureChest.Owner = map;
			treasureChest.GlobalPosition = map.SetGetPickableLoc(player.GlobalPosition, true);

			QuestMaster.CheckQuests(pickableWorldName,
				SpellDB.Instance.HasData(pickableWorldName)
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				false);

			Hide();
		}
	}
}