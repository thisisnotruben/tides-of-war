using Godot;
namespace Game.Ui
{
	public class SpellBookController : GameMenu
	{
		public InventoryModel spellBook = new InventoryModel();
		private ItemInfoSpellController itemInfoSpellController;
		private PopupController popupController;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnSpellBookNodeHide));

			itemInfoSpellController = GetNode<ItemInfoSpellController>("itemInfo");
			itemInfoSpellController.itemList = spellBook;
			itemInfoSpellController.Connect("hide", this, nameof(_OnSpellBookNodeHide));

			// connect slot events
			foreach (Control control in GetNode("s/v/c/InventoryGridView").GetChildren())
			{
				SlotController slot = control as SlotController;
				if (slot != null)
				{
					slot.button.Connect("pressed", this, nameof(_OnSpellBookIndexSelected),
						new Godot.Collections.Array() { slot.GetIndex() });
				}
			}
		}
		public void _OnSpellBookNodeDraw()
		{
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

			Globals.PlaySound("spell_select", this, speaker);
			GetNode<Control>("s").Hide();

			itemInfoSpellController.Display(spellBook.GetCommodity(slotIndex), true);
		}
	}
}
