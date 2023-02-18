using Godot;
using Game.Database;
namespace Game.Ui
{
	public class InventoryController : GameMenu
	{
		public readonly InventoryModel inventory = new InventoryModel();
		public SlotGridController inventorySlots;
		public ItemInfoInventoryController inventoryItemInfo;
		protected Control mainContent;
		protected SlotController weaponSlot, armorSlot;

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);

			weaponSlot = GetNode<SlotController>("s/v/slots/weapon");
			weaponSlot.Connect(nameof(SlotController.OnSlotDragMoved), this, nameof(OnItemDraggedEquip));
			weaponSlot.button.Connect("pressed", this, nameof(OnEquippedSlotPressed),
				new Godot.Collections.Array() { true });

			armorSlot = GetNode<SlotController>("s/v/slots/armor");
			armorSlot.Connect(nameof(SlotController.OnSlotDragMoved), this, nameof(OnItemDraggedEquip));
			armorSlot.button.Connect("pressed", this, nameof(OnEquippedSlotPressed),
				new Godot.Collections.Array() { false });

			inventorySlots = GetNode<SlotGridController>("s/v/c/SlotGrid");
			inventorySlots.Connect(nameof(SlotGridController.UnhandledSlotMove), this, nameof(OnItemDraggedEquip));

			inventoryItemInfo = GetChild<ItemInfoInventoryController>(1);
			inventoryItemInfo.inventoryModel = inventory;
			inventoryItemInfo.slotGridController = inventorySlots;
			inventoryItemInfo.Connect("hide", this, nameof(OnInventoryControllerHide));

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
		public void RefreshSlots()
		{
			if (player.weapon == null)
			{
				weaponSlot.ClearDisplay();
			}
			else
			{
				weaponSlot.Display(player.weapon.worldName);
			}
			if (player.vest == null)
			{
				armorSlot.ClearDisplay();
			}
			else
			{
				armorSlot.Display(player.vest.worldName);
			}

			for (int i = 0; i < inventory.count; i++)
			{
				if (!inventorySlots.IsModelSlotUsed(i))
				{
					inventorySlots.DisplaySlot(
						inventorySlots.GetNextSlot(-1, true, false),
						i,
						inventory.GetCommodity(i),
						inventory.GetCommodityStack(i),
						Globals.cooldownMaster.GetCoolDown(player.GetPath(), inventory.GetCommodity(i))
					);
				}
			}
			inventorySlots.RefreshSlots(inventory);
		}
		private void OnInventoryControllerHide()
		{
			PlaySound(NameDB.UI.MERCHANT_CLOSE);
			inventoryItemInfo.Hide();
			mainContent.Show();
		}
		private void OnInventoryIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (!inventorySlots.IsSlotUsed(slotIndex))
			{
				return;
			}

			PlaySound(NameDB.UI.INVENTORY_OPEN);
			mainContent.Hide();

			inventoryItemInfo.selectedSlotIdx = slotIndex;
			inventoryItemInfo.Display(
				inventory.GetCommodity(inventorySlots.GetSlotToModelIndex(slotIndex))
				, true
			);
		}
		private void OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player?.weapon != null) || (!weapon && player?.vest != null))
			{
				mainContent.Hide();
				inventoryItemInfo.Display(weapon ? player.weapon.worldName : player.vest.worldName, false);
				inventoryItemInfo.dropBttn.Hide();
			}
		}
		private void OnItemDraggedEquip(string itemName, NodePath slotFrom, NodePath slotTo)
		{
			inventoryItemInfo.commodityWorldName = itemName;

			if (slotTo.ToString().Equals(weaponSlot.GetPath().ToString())
			|| slotTo.ToString().Equals(armorSlot.GetPath().ToString()))
			{
				inventoryItemInfo.selectedSlotIdx = GetNode(slotFrom).Owner == inventorySlots
					? inventorySlots.GetSlots().IndexOf(GetNode<SlotController>(slotFrom))
					: inventoryItemInfo.GetSlotIndexFromHud(itemName);
				inventoryItemInfo.OnEquipPressed();
			}
			else
			{
				inventoryItemInfo.selectedSlotIdx = GetNode(slotTo).GetIndex();
				inventoryItemInfo.OnUnequipPressed();
			}
			inventoryItemInfo.selectedSlotIdx = -1;
		}
	}
}