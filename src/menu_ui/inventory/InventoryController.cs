using Godot;
using Game.GameItem;
using Game.Database;
namespace Game.Ui
{
	public class InventoryController : GameMenu
	{
		public readonly InventoryModel inventory = new InventoryModel();
		private SlotGridController inventorySlots;
		public ItemInfoInventoryController itemInfoInventoryController;
		protected Control mainContent;
		protected TextureRect weaponIcon, armorIcon;

		public override void _Ready()
		{
			mainContent = GetNode<Control>("s");

			weaponIcon = GetNode<TextureRect>("s/v/slots/weapon/m/icon");
			armorIcon = GetNode<TextureRect>("s/v/slots/armor/m/icon");

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

			TextureButton weaponSlot = GetNode<TextureButton>("s/v/slots/weapon"),
				armorSlot = GetNode<TextureButton>("s/v/slots/armor");

			weaponSlot.Connect("pressed", this, nameof(_OnEquippedSlotPressed),
				new Godot.Collections.Array() { true });
			armorSlot.Connect("pressed", this, nameof(_OnEquippedSlotPressed),
				new Godot.Collections.Array() { false });

			// connect button effects
			weaponSlot.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { weaponSlot, true });
			armorSlot.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { armorSlot, true });

			weaponSlot.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { weaponSlot, false });
			armorSlot.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { armorSlot, false });
		}
		public void _OnInventoryControllerDraw() { RefreshSlots(); }
		public void RefreshSlots()
		{
			inventorySlots.ClearSlots();
			for (int i = 0; i < inventory.count; i++)
			{
				inventorySlots.DisplaySlot(i, inventory.GetCommodity(i), inventory.GetCommodityStack(i),
					Commodity.GetCoolDown(player.GetPath(), inventory.GetCommodity(i)));
			}
		}
		public void _OnInventoryControllerHide()
		{
			PlaySound(NameDB.UI.MERCHANT_CLOSE);
			itemInfoInventoryController.Hide();
			mainContent.Show();
		}
		public void _OnItemEquipped(string worldName, bool on)
		{
			ItemDB.ItemData itemData = ItemDB.Instance.GetData(worldName);
			(itemData.type == ItemDB.ItemType.ARMOR
				? armorIcon
				: weaponIcon
			).Texture = on ? itemData.icon : null;
		}
		public void _OnInventoryIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (slotIndex >= inventory.count)
			{
				return;
			}

			PlaySound(NameDB.UI.INVENTORY_OPEN);
			mainContent.Hide();

			itemInfoInventoryController.selectedSlotIdx = slotIndex;
			itemInfoInventoryController.Display(inventory.GetCommodity(slotIndex), true);
		}
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				mainContent.Hide();
				itemInfoInventoryController.Display(weapon ? player.weapon.worldName : player.vest.worldName, false);
				itemInfoInventoryController.equipBttn.Hide();
				itemInfoInventoryController.unequipBttn.Show();
			}
		}
	}
}