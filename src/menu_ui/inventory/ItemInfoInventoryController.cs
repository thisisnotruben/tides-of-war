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
		[Signal]
		public delegate void ItemEquipped(string worldName, bool on);
		[Signal]
		public delegate void RefreshSlots();

		public override void _Ready()
		{
			base._Ready();
			GetNode<BaseButton>("s/h/buttons/use")
				.Connect("pressed", this, nameof(_OnUsePressed));
			GetNode<BaseButton>("s/h/buttons/equip")
				.Connect("pressed", this, nameof(_OnEquipPressed));
			GetNode<BaseButton>("s/h/buttons/unequip")
				.Connect("pressed", this, nameof(_OnUnequipPressed));
			GetNode<BaseButton>("s/h/buttons/drop")
				.Connect("pressed", this, nameof(_OnDropPressed));
		}
		private void EquipItem(bool on, string worldName)
		{
			Item item = (Item)new ItemFactory().MakeCommodity(player, worldName);
			switch (ItemDB.GetItemData(worldName).type)
			{
				case ItemDB.ItemType.WEAPON:
					// add back to inventory
					if (!on && player.weapon != null)
					{
						itemList.AddCommodity(player.weapon.worldName);
					}

					player.weapon = (on) ? item : null;
					break;
				case ItemDB.ItemType.ARMOR:
					// add back to inventory
					if (!on && player.vest != null)
					{
						itemList.AddCommodity(player.vest.worldName);
					}

					player.vest = (on) ? item : null;
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
			string sndName = "click2";

			switch (ItemDB.GetItemData(pickableWorldName).type)
			{
				case ItemDB.ItemType.FOOD:
					sndName = "eat";
					// cannot eat in combat
					if (player.attacking)
					{
						GetNode<Control>("s").Hide();
						popupController.GetNode<Label>("m/error/label").Text = "Cannot Eat\nIn Combat!";
						popupController.GetNode<Control>("m/error").Show();
						popupController.Show();
						return;
					}
					break;
				case ItemDB.ItemType.POTION:
					sndName = "drink";
					break;
			}
			Globals.soundPlayer.PlaySound(sndName);

			// eat up or drink up
			new ItemFactory().MakeCommodity(player, pickableWorldName).Start();
			// remove from inventory
			itemList.RemoveCommodity(pickableWorldName);

			EmitSignal(nameof(RefreshSlots));
			Hide();
		}
		public void _OnEquipPressed()
		{
			ItemDB.ItemType itemType = ItemDB.GetItemData(pickableWorldName).type;
			Item playerWeapon = player.weapon;
			Item playerArmor = player.vest;
			bool inventoryFull = false;

			// unequip weapon if any to equip focused weapon
			if (itemType == ItemDB.ItemType.WEAPON && playerWeapon != null)
			{
				if (itemList.IsFull(playerWeapon.worldName))
				{
					inventoryFull = true;
				}
				else
				{
					EquipItem(false, playerWeapon.worldName);
				}
			}

			// unequip armor if any to equip focused armor
			if (itemType == ItemDB.ItemType.ARMOR && playerArmor != null)
			{
				if (itemList.IsFull(playerArmor.worldName))
				{
					inventoryFull = true;
				}
				else
				{
					EquipItem(false, playerArmor.worldName);
				}
			}

			if (inventoryFull)
			{
				// inventory full
				GetNode<Control>("s").Hide();
				popupController.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				Globals.soundPlayer.PlaySound(ItemDB.GetItemData(pickableWorldName).material + "_off");
				// equip focused item
				EquipItem(true, pickableWorldName);
			}
		}
		public void _OnUnequipPressed()
		{
			if (itemList.IsFull(pickableWorldName))
			{
				GetNode<Control>("s").Hide();
				popupController.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				Globals.soundPlayer.PlaySound("inventory_unequip");
				EquipItem(false, pickableWorldName);
			}
		}
		public void _OnDropPressed()
		{
			Globals.soundPlayer.PlaySound("click2");

			RouteConnections(nameof(_OnDropConfirm));
			GetNode<Control>("s").Hide();
			popupController.GetNode<Label>("m/yes_no/label").Text = "Drop?";
			popupController.GetNode<Control>("m/yes_no").Show();
			popupController.Show();
		}
		public void _OnDropConfirm()
		{
			foreach (string sndName in new string[] { "click2", "inventory_drop" })
			{
				Globals.soundPlayer.PlaySound(sndName);
			}

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
				SpellDB.HasSpell(pickableWorldName)
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				false);

			Hide();
		}
	}
}