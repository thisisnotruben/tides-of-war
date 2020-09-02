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
			foreach (Control control in GetNode("s/v/c/SlotGrid").GetChildren())
			{
				SlotController slot = control as SlotController;
				if (slot != null)
				{
					slot.button.Connect("pressed", this, nameof(_OnInventoryIndexSelected),
						new Godot.Collections.Array() { slot.GetIndex() });
				}
			}
		}
		public void _OnInventoryControllerDraw()
		{
			inventorySlots.ClearSlots();
			for (int i = 0; i < inventory.count; i++)
			{
				inventorySlots.DisplaySlot(i, inventory.GetCommodity(i), inventory.GetCommodityStack(i));
			}
		}
		public void _OnInventoryControllerHide()
		{
			// TODO: new sound please
			// Globals.PlaySound("merchant_close", this, speaker);
			itemInfoInventoryController.Hide();
			GetNode<Control>("s").Show();
		}
		public void _OnItemEquipped(Commodity item, bool on)
		{
			ItemDB.ItemNode itemNode = ItemDB.GetItemData(item.worldName);

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
			string pickableWorldName = inventory.GetCommodity(slotIndex);
			GetNode<Control>("s").Hide();
			itemInfoInventoryController.Display(pickableWorldName, true);
		}
		public void _OnEquippedSlotMoved(string nodePath, bool down)
		{
			float scale = (down) ? 0.8f : 1.0f;
			GetNode<Control>(nodePath).RectScale = new Vector2(scale, scale);
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
