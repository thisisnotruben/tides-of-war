using Game.GameItem;
using Game.Database;
using Godot;
namespace Game.Ui
{
	public class SpellBookController : GameMenu
	{
		public readonly InventoryModel spellBook = new InventoryModel();
		public SlotGridController spellSlots;
		private ItemInfoSpellController spellInfo;
		private PopupController popup;
		private Control mainContent;

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);
			spellSlots = mainContent.GetChild<SlotGridController>(0);

			spellInfo = GetChild<ItemInfoSpellController>(1);
			spellInfo.inventoryModel = spellBook;
			spellInfo.slotGridController = spellSlots;
			spellInfo.backBttn.Connect("pressed", this, nameof(OnItemInfoBackPressed));

			popup = GetChild<PopupController>(2);
			popup.Connect("hide", this, nameof(OnHide));

			// connect slot events
			foreach (SlotController slot in spellSlots.GetSlots())
			{
				slot.button.Connect("pressed", this, nameof(OnSpellBookIndexSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}
		}
		private void OnDraw()
		{
			for (int i = 0; i < spellBook.count; i++)
			{
				if (!spellSlots.IsModelSlotUsed(i))
				{
					spellSlots.DisplaySlot(
						spellSlots.GetNextSlot(-1, true, false),
						i,
						spellBook.GetCommodity(i),
						spellBook.GetCommodityStack(i),
						Commodity.GetCoolDown(player.GetPath(), spellBook.GetCommodity(i))
					);
				}
			}
			spellSlots.RefreshSlots(spellBook);
		}
		private void OnHide() { popup.Hide(); }
		private void OnItemInfoBackPressed() { mainContent.Show(); }
		private void OnSpellBookIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (!spellSlots.IsSlotUsed(slotIndex))
			{
				return;
			}

			PlaySound(NameDB.UI.SPELL_SELECT);
			mainContent.Hide();

			spellInfo.selectedSlotIdx = slotIndex;
			spellInfo.Display(spellBook.GetCommodity(spellSlots.GetSlotToModelIndex(slotIndex)), true);
		}
	}
}