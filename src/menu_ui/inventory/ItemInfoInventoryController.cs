using Godot;
using Game.Loot;
using Game.ItemPoto;
using Game.Database;
using Game.Actor.State;
namespace Game.Ui
{
	public class ItemInfoInventoryController : ItemInfoController
	{
		[Signal]
		public delegate void ItemEquipped(Commodity item, bool on);

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
		public void EquipItem(bool on)
		{
			ItemDB.ItemNode itemNode = ItemDB.GetItemData(pickableWorldName);
			Commodity item = PickableFactory.MakeCommodity(pickableWorldName);
			switch (itemNode.type)
			{
				case ItemDB.ItemType.WEAPON:
					player.weapon = (on) ? item : null;
					break;
				case ItemDB.ItemType.ARMOR:
					player.vest = (on) ? item : null;
					break;
			}
			if (on)
			{
				// TODO: refresh slots
				itemList.RemoveCommodity(pickableWorldName);
			}
			else
			{
				itemList.AddCommodity(pickableWorldName);
				// ?TODO: confused here as well
				// ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
				// foreach (ItemSlot otherItemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
				// {
				// 	if (otherItemSlot.GetItem().Equals(pickableWorldName)
				// 	&& !itemSlot.IsConnected(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut)))
				// 	{
				// 		itemSlot.Connect(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut));
				// 	}
				// }
			}
			EmitSignal(nameof(ItemEquipped), item, on);
		}
		public void _OnUsePressed()
		{
			ItemDB.ItemType itemType = ItemDB.GetItemData(pickableWorldName).type;
			string sndName = "click2";

			switch (itemType)
			{
				case ItemDB.ItemType.FOOD:
					sndName = "eat";
					if (player.state == FSM.State.ATTACK)
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
			Globals.PlaySound(sndName, this, speaker);
			// TODO: going to need to get slot from inventory/spell bag
			// if (itemType.Equals("POTION"))
			// {
			//     itemList.SetSlotCoolDown(item.worldName, item.duration, 0.0f);
			// }
			// itemList.RemoveItem(pickableWorldName, true, false, false);
			// item.Consume(player, 0.0f);
			Hide();
		}
		public void _OnEquipPressed()
		{
			ItemDB.ItemType itemType = ItemDB.GetItemData(pickableWorldName).type;
			string currentPickableWorldName = pickableWorldName;
			Commodity playerWeapon = player.weapon;
			Commodity playerArmor = player.vest;
			if (!itemList.IsFull())
			{
				switch (itemType)
				{
					case ItemDB.ItemType.WEAPON:

						if (playerWeapon != null)
						{
							pickableWorldName = playerWeapon.worldName;
							EquipItem(false);
							pickableWorldName = currentPickableWorldName;
						}
						break;
					case ItemDB.ItemType.ARMOR:
						if (playerArmor != null)
						{
							pickableWorldName = playerArmor.worldName;
							EquipItem(false);
							pickableWorldName = currentPickableWorldName;
						}
						break;
				}
			}
			else if ((itemType == ItemDB.ItemType.WEAPON & playerWeapon != null)
			|| (itemType == ItemDB.ItemType.ARMOR & playerArmor != null))
			{
				GetNode<Control>("s").Hide();
				popupController.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				// ?TODO: I'm confused here
				// ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
				// itemSlot.SetBlockSignals(true);
				EquipItem(true);
				// itemSlot.SetBlockSignals(false);
				Globals.PlaySound(ItemDB.GetItemData(pickableWorldName).material + "_off", this, speaker);
				Hide();
			}
		}
		public void _OnUnequipPressed()
		{
			if (itemList.IsFull())
			{
				GetNode<Control>("s").Hide();
				popupController.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				Globals.PlaySound("inventory_unequip", this, speaker);
				EquipItem(false);
			}
		}
		public void _OnDropPressed()
		{
			RouteConnections(nameof(_OnDropConfirm));
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("s").Hide();
			popupController.GetNode<Label>("m/yes_no/label").Text = "Drop?";
			popupController.GetNode<Control>("m/yes_no").Show();
			popupController.Show();
		}
		public void _OnDropConfirm()
		{
			foreach (string sndName in new string[] { "click2", "inventory_drop" })
			{
				Globals.PlaySound(sndName, this, speaker);
			}
			itemList.RemoveCommodity(pickableWorldName);
			PackedScene lootScene = (PackedScene)GD.Load("res://src/loot/loot_chest.tscn");
			LootChest loot = (LootChest)lootScene.Instance();
			loot.pickableWorldName = pickableWorldName;
			Map.Map map = Map.Map.map;
			map.AddZChild(loot);
			loot.Owner = map;
			loot.GlobalPosition = map.SetGetPickableLoc(player.GlobalPosition, true);
			Hide();
		}
	}
}