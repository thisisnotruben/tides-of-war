using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemInfoMerchantController : ItemInfoController
	{
		public InventoryModel playerSpellBook;
		public bool isBuying;

		[Signal] public delegate void OnTransaction(string pickableWorldName, int goldAmount, bool bought);

		public override void _Ready()
		{
			base._Ready();

			buyBttn.Connect("pressed", this, nameof(_OnBuyPressed));
			sellBttn.Connect("pressed", this, nameof(_OnSellPressed));

			BaseButton addToHudBttn = GetNode<BaseButton>("s/v/c/v/add_to_hud");
			addToHudBttn.Disconnect("button_down", this, nameof(_OnSlotMoved));
			addToHudBttn.Disconnect("button_up", this, nameof(_OnSlotMoved));
			addToHudBttn.Disconnect("pressed", this, nameof(_OnAddToHudPressed));
		}
		public override void _OnMovePressed(int by)
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);

			// get next commodity when pressing arrows
			selectedSlotIdx += by;
			string nextCommodityWorldName = itemList.GetCommodity(selectedSlotIdx);

			bool playerHaveSpell = SpellDB.Instance.HasData(nextCommodityWorldName)
				&& playerSpellBook.HasItem(nextCommodityWorldName);

			Display(nextCommodityWorldName, true, isBuying, playerHaveSpell);
		}
		public void Display(string pickableWorldName, bool allowMove, bool buy, bool alreadyHave)
		{
			Display(pickableWorldName, allowMove);

			// set which buttons to show accordingly
			buyBttn.Text = SpellDB.Instance.HasData(pickableWorldName) ? "Train" : "Buy";
			HideExcept(alreadyHave ? new Control[] { } : new Control[] { buy ? buyBttn : sellBttn });
		}
		public void _OnBuyPressed()
		{
			RouteConnections(nameof(_OnBuyConfirm));
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			if (SpellDB.Instance.HasData(pickableWorldName)
			&& player.level < SpellDB.Instance.GetData(pickableWorldName).level)
			{
				popupController.errorLabel.Text = "Can't Learn\nThis Yet!";
				popupController.errorView.Show();

			}
			else if (PickableDB.GetGoldCost(pickableWorldName) < player.gold)
			{
				popupController.yesNoLabel.Text = SpellDB.Instance.HasData(pickableWorldName) ? "Learn?" : "Buy?";
				popupController.yesNoView.Show();
			}
			else
			{
				popupController.errorLabel.Text = "Not Enough\nGold!";
				popupController.errorView.Show();
			}
			popupController.Show();
		}
		public void _OnBuyConfirm()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.SELL_BUY);
			if (popupController.yesNoLabel.Text.Equals("Learn?"))
			{
				Globals.soundPlayer.PlaySound(NameDB.UI.LEARN_SPELL);
				buyBttn.Hide();
			}
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, -PickableDB.GetGoldCost(pickableWorldName), true);
			popupController.Hide();
		}
		public void _OnSellPressed()
		{
			RouteConnections(nameof(_OnSellConfirm));
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			popupController.yesNoLabel.Text = "Sell?";
			popupController.yesNoView.Show();
			popupController.Show();
		}
		public void _OnSellConfirm()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.SELL_BUY);
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, PickableDB.GetGoldCost(pickableWorldName), false);
			Hide();
		}
	}
}