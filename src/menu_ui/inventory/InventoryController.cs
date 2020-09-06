using Godot;
using Game.ItemPoto;
using Game.Database;
namespace Game.Ui
{
	public class InventoryController : GameMenu
	{
		public readonly InventoryModel inventory = new InventoryModel();
		private SlotGridController inventorySlots;
		private ItemInfoInventoryController itemInfoInventoryController;

		public override void _Ready()
		{
			inventorySlots = GetNode<SlotGridController>("s/v/c/SlotGrid");
			Connect("draw", this, nameof(_OnInventoryControllerDraw));

			itemInfoInventoryController = GetNode<ItemInfoInventoryController>("item_info");
			itemInfoInventoryController.itemList = inventory;
			itemInfoInventoryController.Connect("hide", this, nameof(_OnInventoryControllerHide));
			itemInfoInventoryController.Connect(nameof(ItemInfoInventoryController.ItemEquipped),
				this, nameof(_OnItemEquipped));

			// connect slot events
			foreach (SlotController slot in inventorySlots.GetSlots())
			{
				slot.button.Connect("pressed", this, nameof(_OnInventoryIndexSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}

			// refresh slots whenever an item is dropped from inventory
			itemInfoInventoryController.Connect(nameof(ItemInfoInventoryController.RefreshSlots),
				this, nameof(RefreshSlots));
		}
		public void _OnInventoryControllerDraw() { RefreshSlots(); }
		public void RefreshSlots()
		{
			inventorySlots.ClearSlots();
			for (int i = 0; i < inventory.count; i++)
			{
				inventorySlots.DisplaySlot(i, inventory.GetCommodity(i), inventory.GetCommodityStack(i),
					Commodity.GetCoolDown(player, inventory.GetCommodity(i)));
			}
		}
		public void _OnInventoryControllerHide()
		{
			Globals.PlaySound("merchant_close", this, speaker);
			itemInfoInventoryController.Hide();
			GetNode<Control>("s").Show();
		}
		public void _OnItemEquipped(string worldName, bool on)
		{
			ItemDB.ItemNode itemNode = ItemDB.GetItemData(worldName);

			GetNode<TextureRect>(
				(itemNode.type == ItemDB.ItemType.ARMOR)
				? "s/v/slots/armor/m/icon"
				: "s/v/slots/weapon/m/icon")
					.Texture = (on) ? itemNode.icon : null;
		}
		public void _OnInventoryIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (slotIndex >= inventory.count)
			{
				return;
			}

			Globals.PlaySound("inventory_open", this, speaker);
			GetNode<Control>("s").Hide();

			itemInfoInventoryController.selectedSlotIdx = slotIndex;
			itemInfoInventoryController.Display(inventory.GetCommodity(slotIndex), true);
		}
		public void _OnEquippedSlotMoved(string nodePath, bool down)
		{
			GetNode<Control>(nodePath).RectScale = (down) ? new Vector2(0.8f, 0.8f) : new Vector2(1.0f, 1.0f);
		}
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				itemInfoInventoryController.Display((weapon) ? player.weapon.worldName : player.vest.worldName, false);
			}
		}
	}
}
