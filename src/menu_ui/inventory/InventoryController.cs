using Godot;
using Game.GameItem;
using Game.Database;
namespace Game.Ui
{
	public class InventoryController : GameMenu
	{
		public readonly InventoryModel inventory = new InventoryModel();
		public SlotGridController inventorySlots;
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
		private void RefreshSlots()
		{
			Item playerWeapon = player.weapon,
				playerArmor = player.vest;

			weaponIcon.Texture = playerWeapon == null
				? null
				: Globals.itemDB.GetData(playerWeapon.worldName).icon;
			armorIcon.Texture = playerArmor == null
				? null
				: Globals.itemDB.GetData(playerArmor.worldName).icon;

			for (int i = 0; i < inventory.count; i++)
			{
				if (!inventorySlots.IsModelSlotUsed(i))
				{
					inventorySlots.DisplaySlot(
						inventorySlots.GetNextSlot(-1, true, false),
						i,
						inventory.GetCommodity(i),
						inventory.GetCommodityStack(i),
						Commodity.GetCoolDown(player.GetPath(), inventory.GetCommodity(i))
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
			}
		}
	}
}