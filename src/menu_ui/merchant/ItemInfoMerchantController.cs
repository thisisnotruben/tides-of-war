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

			buyBttn.Connect("pressed", this, nameof(_OnBuyPressed));
			sellBttn.Connect("pressed", this, nameof(_OnSellPressed));

			addToHudBttn.Disconnect("button_down", this, nameof(OnSlotMoved));
			addToHudBttn.Disconnect("button_up", this, nameof(OnSlotMoved));
			addToHudBttn.Disconnect("pressed", this, nameof(OnAddToHudPressed));
		}
		public override void _OnMovePressed(int by)
		{
			PlaySound(NameDB.UI.CLICK2);

			// get next commodity when pressing arrows
			selectedSlotIdx += by;
			string nextCommodityWorldName = itemList.GetCommodity(selectedSlotIdx);

			bool playerHaveSpell = Globals.spellDB.HasData(nextCommodityWorldName)
				&& playerSpellBook.HasItem(nextCommodityWorldName);

			Display(nextCommodityWorldName, true, isBuying, playerHaveSpell);
		}
		public void Display(string commodityWorldName, bool allowMove, bool buy, bool alreadyHave)
		{
			Display(commodityWorldName, allowMove);

			// set which buttons to show accordingly
			buyBttn.Text = Globals.spellDB.HasData(commodityWorldName) ? "Train" : "Buy";
			HideExcept(alreadyHave ? new Control[] { } : new Control[] { buy ? buyBttn : sellBttn });
		}
		public void _OnBuyPressed()
		{
			RouteConnections(nameof(_OnBuyConfirm));
			PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			if (Globals.spellDB.HasData(commodityWorldName)
			&& player.level < Globals.spellDB.GetData(commodityWorldName).level)
			{
				popupController.ShowError("Can't Learn\nThis Yet!");

			}
			else if (PickableDB.GetGoldCost(commodityWorldName) < player.gold)
			{
				popupController.ShowConfirm(Globals.spellDB.HasData(commodityWorldName) ? "Learn?" : "Buy?");
			}
			else
			{
				popupController.ShowError("Not Enough\nGold!");
			}
		}
		public void _OnBuyConfirm()
		{
			PlaySound(NameDB.UI.SELL_BUY);
			if (popupController.yesNoLabel.Text.Equals("Learn?"))
			{
				PlaySound(NameDB.UI.LEARN_SPELL);
				buyBttn.Hide();
			}
			EmitSignal(nameof(OnTransaction), commodityWorldName, -PickableDB.GetGoldCost(commodityWorldName), true);
			popupController.Hide();
		}
		public void _OnSellPressed()
		{
			RouteConnections(nameof(_OnSellConfirm));
			PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			popupController.ShowConfirm("Sell?");
		}
		public void _OnSellConfirm()
		{
			PlaySound(NameDB.UI.SELL_BUY);
			EmitSignal(nameof(OnTransaction), commodityWorldName, PickableDB.GetGoldCost(commodityWorldName), false);
			Hide();
		}
	}
}