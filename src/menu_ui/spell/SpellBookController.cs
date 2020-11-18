using Game.GameItem;
using Game.Database;
using Godot;
namespace Game.Ui
{
	public class SpellBookController : GameMenu
	{
		public InventoryModel spellBook = new InventoryModel();
		private SlotGridController spellSlots;
		private ItemInfoSpellController itemInfoSpellController;
		private PopupController popupController;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnSpellBookNodeHide));

			itemInfoSpellController = GetNode<ItemInfoSpellController>("itemInfo");
			itemInfoSpellController.itemList = spellBook;
			itemInfoSpellController.Connect("hide", this, nameof(_OnSpellBookNodeHide));

			spellSlots = GetNode<SlotGridController>("s/v/c/InventoryGridView");

			// connect slot events
			foreach (SlotController slot in spellSlots.GetSlots())
			{
				slot.button.Connect("pressed", this, nameof(_OnSpellBookIndexSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}
		}
		public void _OnSpellBookNodeDraw()
		{
			// display slots
			spellSlots.ClearSlots();
			for (int i = 0; i < spellBook.count; i++)
			{
				spellSlots.DisplaySlot(i, spellBook.GetCommodity(i), spellBook.GetCommodityStack(i),
					Commodity.GetCoolDown(player.GetPath(), spellBook.GetCommodity(i)));
			}

			// fill hp/mana headers
			GetNode<Label>("s/v/m/v/playerHpHeader").Text = $"Health: {player.hp} / {player.stats.hpMax.valueI}";
			GetNode<Label>("s/v/m/v/playerManaHeader").Text = $"Mana: {player.mana} / {player.stats.manaMax.valueI}";
		}
		public void _OnSpellBookNodeHide()
		{
			popupController.Hide();
			GetNode<Control>("s").Show();
		}
		public void _OnSpellBookIndexSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (slotIndex >= spellBook.count)
			{
				return;
			}

			Globals.soundPlayer.PlaySound(NameDB.UI.SPELL_SELECT);
			GetNode<Control>("s").Hide();

			itemInfoSpellController.selectedSlotIdx = slotIndex;
			itemInfoSpellController.Display(spellBook.GetCommodity(slotIndex), true);
		}
		public override void _OnBackPressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.SPELL_BOOK_CLOSE);
			Hide();
		}
	}
}