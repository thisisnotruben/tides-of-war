using Godot;
using Game.GameItem;
using Game.Database;
namespace Game.Ui
{
	public class InventoryController : GameMenu
	{
		public readonly InventoryModel inventory = new InventoryModel();
		private SlotGridController inventorySlots;
		public ItemInfoInventoryController inventoryItemInfo;
		protected Control mainContent;
		protected TextureRect weaponIcon, armorIcon;

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);

			weaponIcon = GetNode<TextureRect>("s/v/slots/weapon/m/icon");
			armorIcon = GetNode<TextureRect>("s/v/slots/armor/m/icon");

			inventorySlots = GetNode<SlotGridController>("s/v/c/SlotGrid");

			inventoryItemInfo = GetChild<ItemInfoInventoryController>(1);
			inventoryItemInfo.itemList = inventory;
			inventoryItemInfo.Connect("hide", this, nameof(OnInventoryControllerHide));
			inventoryItemInfo.Connect(nameof(ItemInfoInventoryController.ItemEquipped), this, nameof(OnItemEquipped));

			// connect slot events
			foreach (SlotController slot in inventorySlots.GetSlots())
			{
				slot.button.Connect("pressed", this, nameof(OnInventoryIndexSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}

			// refresh slots whenever an item is dropped from inventory
			inventoryItemInfo.Connect(nameof(ItemInfoInventoryController.RefreshSlots), this, nameof(RefreshSlots));
		}
		private void OnDraw() { RefreshSlots(); }
		private void RefreshSlots()
		{
			inventorySlots.ClearSlots();
			for (int i = 0; i < inventory.count; i++)
			{
				inventorySlots.DisplaySlot(i, inventory.GetCommodity(i), inventory.GetCommodityStack(i),
					Commodity.GetCoolDown(player.GetPath(), inventory.GetCommodity(i)));
			}
		}
		private void OnInventoryControllerHide()
		{
			PlaySound(NameDB.UI.MERCHANT_CLOSE);
			inventoryItemInfo.Hide();
			mainContent.Show();
		}
		private void OnItemEquipped(string worldName, bool on)
		{
			ItemDB.ItemData itemData = Globals.itemDB.GetData(worldName);
			(itemData.type == ItemDB.ItemType.ARMOR
				? armorIcon
				: weaponIcon
			).Texture = on ? itemData.icon : null;
		}
		private void OnInventoryIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (slotIndex >= inventory.count)
			{
				return;
			}

			PlaySound(NameDB.UI.INVENTORY_OPEN);
			mainContent.Hide();

			inventoryItemInfo.selectedSlotIdx = slotIndex;
			inventoryItemInfo.Display(inventory.GetCommodity(slotIndex), true);
		}
		private void OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player?.weapon != null) || (!weapon && player?.vest != null))
			{
				mainContent.Hide();
				inventoryItemInfo.Display(weapon ? player.weapon.worldName : player.vest.worldName, false);
				inventoryItemInfo.equipBttn.Hide();
				inventoryItemInfo.unequipBttn.Show();
			}
		}
	}
}