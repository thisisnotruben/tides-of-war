using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemInfoMerchantController : ItemInfoController
	{
		public InventoryModel playerSpellBook;
		public bool isBuying = false;
		[Signal]
		public delegate void OnTransaction(string pickableWorldName, int goldAmount, bool bought);

		public override void _Ready()
		{
			base._Ready();
			GetNode<BaseButton>("s/h/buttons/buy")
				.Connect("pressed", this, nameof(_OnBuyPressed));
			GetNode<BaseButton>("s/h/buttons/sell")
				.Connect("pressed", this, nameof(_OnSellPressed));
			BaseButton addToHudBttn = GetNode<BaseButton>("s/v/c/v/add_to_hud");
			addToHudBttn.Disconnect("button_down", this, nameof(_OnSlotMoved));
			addToHudBttn.Disconnect("button_up", this, nameof(_OnSlotMoved));
			addToHudBttn.Disconnect("pressed", this, nameof(_OnAddToHudPressed));
		}
		public override void _OnMovePressed(int by)
		{
			Globals.soundPlayer.PlaySound("click2");

			// get next commodity when pressing arrows
			selectedSlotIdx += by;
			string nextCommodityWorldName = itemList.GetCommodity(selectedSlotIdx);

			bool playerHaveSpell = SpellDB.HasSpell(nextCommodityWorldName)
				&& playerSpellBook.HasItem(nextCommodityWorldName);

			Display(nextCommodityWorldName, true, isBuying, playerHaveSpell);
		}
		public void Display(string pickableWorldName, bool allowMove, bool buy, bool alreadyHave)
		{
			// call overloaded method
			Display(pickableWorldName, allowMove);

			// set which buttons to show accordingly
			GetNode<Label>("s/h/buttons/buy/label").Text = SpellDB.HasSpell(pickableWorldName) ? "Train" : "Buy";
			HideExcept((alreadyHave) ? new string[] { } : new string[] { (buy) ? "buy" : "sell" });
		}
		public void _OnBuyPressed()
		{
			RouteConnections(nameof(_OnBuyConfirm));
			Globals.soundPlayer.PlaySound("click2");
			GetNode<Control>("s").Hide();
			if (SpellDB.HasSpell(pickableWorldName)
			&& player.level < SpellDB.GetSpellData(pickableWorldName).level)
			{
				popupController.GetNode<Label>("m/error/label").Text = "Can't Learn\nThis Yet!";
				popupController.GetNode<Control>("m/error").Show();

			}
			else if (PickableDB.GetGoldCost(pickableWorldName) < player.gold)
			{
				popupController.GetNode<Label>("m/yes_no/label").Text = (SpellDB.HasSpell(pickableWorldName)) ? "Learn?" : "Buy?";
				popupController.GetNode<Control>("m/yes_no").Show();
			}
			else
			{
				popupController.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
				popupController.GetNode<Control>("m/error").Show();
			}
			popupController.Show();
		}
		public void _OnBuyConfirm()
		{
			Globals.soundPlayer.PlaySound("sell_buy");
			if (popupController.GetNode<Label>("m/yes_no/label").Text.Equals("Learn?"))
			{
				Globals.soundPlayer.PlaySound("learn_spell");
				GetNode<Control>("s/h/buttons/buy").Hide();
			}
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, -PickableDB.GetGoldCost(pickableWorldName), true);
			popupController.Hide();
		}
		public void _OnSellPressed()
		{
			RouteConnections(nameof(_OnSellConfirm));
			Globals.soundPlayer.PlaySound("click2");
			GetNode<Control>("s").Hide();
			popupController.GetNode<Label>("m/yes_no/label").Text = "Sell?";
			popupController.GetNode<Control>("m/yes_no").Show();
			popupController.Show();
		}
		public void _OnSellConfirm()
		{
			Globals.soundPlayer.PlaySound("sell_buy");
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, PickableDB.GetGoldCost(pickableWorldName), false);
			Hide();
		}
	}
}