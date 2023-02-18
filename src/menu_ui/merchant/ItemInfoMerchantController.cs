using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemInfoMerchantController : ItemInfoController
	{
		public InventoryModel playerSpellBook;
		public bool isBuying;

		[Signal] public delegate void OnTransaction(string commodityWorldName, int goldAmount, bool bought);

		public override void _Ready()
		{
			base._Ready();

			iconView.allowDrag = false;
			buyBttn.Connect("pressed", this, nameof(OnBuyPressed));
			sellBttn.Connect("pressed", this, nameof(OnSellPressed));
		}
		public override void OnMovePressed(bool forward)
		{
			PlaySound(NameDB.UI.CLICK2);

			// get next commodity when pressing arrows
			selectedSlotIdx = slotGridController.GetNextSlot(selectedSlotIdx, forward);

			string nextCommodityWorldName = inventoryModel.GetCommodity(
				slotGridController.GetSlotToModelIndex(selectedSlotIdx));

			bool playerHaveSpell = Globals.spellDB.HasData(nextCommodityWorldName)
				&& playerSpellBook.HasItem(nextCommodityWorldName);

			Display(nextCommodityWorldName, true, isBuying, playerHaveSpell);
		}
		public void Display(string commodityWorldName, bool allowMove, bool buy, bool alreadyHave)
		{
			Display(commodityWorldName, allowMove);

			// set which buttons to show accordingly
			buyBttn.Text = Globals.spellDB.HasData(commodityWorldName) ? "Learn" : "Buy";
			HideExcept(alreadyHave ? new Control[] { } : new Control[] { buy ? buyBttn : sellBttn });
		}
		private void OnBuyPressed()
		{
			popup.RouteConnection(nameof(OnBuyConfirm), this);
			PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			if (Globals.spellDB.HasData(commodityWorldName)
			&& player.level < Globals.spellDB.GetData(commodityWorldName).level)
			{
				popup.ShowError("Can't Learn\nThis Yet!");

			}
			else if (PickableDB.GetGoldCost(commodityWorldName) < player.gold)
			{
				popup.ShowConfirm(Globals.spellDB.HasData(commodityWorldName) ? "Learn?" : "Buy?");
			}
			else
			{
				popup.ShowError("Not Enough\nGold!");
			}
		}
		private void OnBuyConfirm()
		{
			PlaySound(NameDB.UI.SELL_BUY);
			if (popup.yesNoLabel.Text.Equals("Learn?"))
			{
				PlaySound(NameDB.UI.LEARN_SPELL);
				buyBttn.Hide();
			}
			EmitSignal(nameof(OnTransaction), commodityWorldName,
				-PickableDB.GetGoldCost(commodityWorldName), true, selectedSlotIdx);
			popup.Hide();
		}
		private void OnSellPressed()
		{
			popup.RouteConnection(nameof(_OnSellConfirm), this);
			PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			popup.ShowConfirm("Sell?");
		}
		private void _OnSellConfirm()
		{
			PlaySound(NameDB.UI.SELL_BUY);
			EmitSignal(nameof(OnTransaction), commodityWorldName,
				PickableDB.GetGoldCost(commodityWorldName), false, selectedSlotIdx);
			Hide();
		}
	}
}