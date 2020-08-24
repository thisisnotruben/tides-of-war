using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemInfoNodeMerchant : ItemInfoNode
	{
		public ItemList spellBookItemList;
		public bool isBuying;
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
			isBuying = false;
		}
		public override void _OnMovePressed(int by)
		{
			Globals.PlaySound("click2", this, speaker);
			string nextPickableWorldName = itemList.GetItemMetaData(
				itemList.GetItemSlot(pickableWorldName).GetIndex() + by);
			bool alreadyHave = SpellDB.HasSpell(nextPickableWorldName)
				&& spellBookItemList.HasItem(nextPickableWorldName, false);
			Display(nextPickableWorldName, true, isBuying, alreadyHave);
		}
		public void Display(string pickableWorldName, bool allowMove, bool buy, bool alreadyHave)
		{
			Display(pickableWorldName, allowMove);
			GetNode<Label>("s/h/buttons/buy/label").Text = SpellDB.HasSpell(pickableWorldName) ? "Train" : "Buy";
			string[] nodesToShow = { (buy) ? "buy" : "sell" };
			if (alreadyHave)
			{
				nodesToShow = new string[] { };
			}
			HideExcept(nodesToShow);
		}
		public void _OnBuyPressed()
		{
			RouteConnections(nameof(_OnBuyConfirm));
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("s").Hide();
			if (SpellDB.HasSpell(pickableWorldName)
			&& player.level < SpellDB.GetSpellData(pickableWorldName).level)
			{
				popup.GetNode<Label>("m/error/label").Text = "Can't Learn\nThis Yet!";
				popup.GetNode<Control>("m/error").Show();

			}
			else if (PickableDB.GetGoldCost(pickableWorldName) < player.gold)
			{
				popup.GetNode<Label>("m/yes_no/label").Text = (SpellDB.HasSpell(pickableWorldName)) ? "Learn?" : "Buy?";
				popup.GetNode<Control>("m/yes_no").Show();
			}
			else
			{
				popup.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
				popup.GetNode<Control>("m/error").Show();
			}
			popup.Show();
		}
		public void _OnBuyConfirm()
		{
			Globals.PlaySound("sell_buy", this, speaker);
			if (popup.GetNode<Label>("m/yes_no/label").Text.Equals("Learn?"))
			{
				Globals.PlaySound("learn_spell", this, speaker);
				GetNode<Control>("s/h/buttons/buy").Hide();
			}
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, -PickableDB.GetGoldCost(pickableWorldName), true);
			popup.Hide();
		}
		public void _OnSellPressed()
		{
			RouteConnections(nameof(_OnSellConfirm));
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("s").Hide();
			popup.GetNode<Label>("m/yes_no/label").Text = "Sell?";
			popup.GetNode<Control>("m/yes_no").Show();
			popup.Show();
		}
		public void _OnSellConfirm()
		{
			Globals.PlaySound("sell_buy", this, speaker);
			EmitSignal(nameof(OnTransaction),
				pickableWorldName, PickableDB.GetGoldCost(pickableWorldName), false);
			Hide();
		}
	}
}