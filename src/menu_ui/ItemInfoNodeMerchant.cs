using Godot;
using Game.Ability;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
    public class ItemInfoNodeMerchant : ItemInfoNode
    {
        public override void _Ready()
        {
            base._Ready();
            GetNode<BaseButton>("s/h/buttons/buy")
                .Connect("pressed", this, nameof(_OnBuyPressed));
            GetNode<BaseButton>("s/h/buttons/sell")
                .Connect("pressed", this, nameof(_OnSellPressed));
        }
        public void Display(Loot.Pickable pickable, bool allowMove, bool buy, bool alreadyHave)
        {
            Display(pickable, allowMove);
            GetNode<Label>("s/h/buttons/buy").Text = (pickable is Spell) ? "Train": "Buy";
            string[] nodesToShow = {(buy) ? "buy" : "sell"};
            if (alreadyHave)
            {
                nodesToShow = new string[] {};
            }
            HideExcept(nodesToShow);
        }
        public void _OnBuyPressed()
        {
            RouteConnections(nameof(_OnBuyConfirm));
            Globals.PlaySound("click2", this, speaker);
            GetNode<Control>("s").Hide();
            if (pickable is Spell && player.level < pickable.level)
            {
                popup.GetNode<Label>("m/error/label").Text = "Can't Learn\nThis Yet!";
                popup.GetNode<Control>("m/error").Show();
            }
            else if (pickable.GetGold() < player.gold)
            {
                popup.GetNode<Label>("m/yes_no/label").Text = (pickable is Item) ? "Buy?" : "Learn?";
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
            if (GetNode<Label>("m/yes_no/label").Text.Equals("Learn?"))
            {
                Globals.PlaySound("learn_spell", this, speaker);
            }
            if (pickable is Item)
            {
                pickable = PickableFactory.GetMakeItem(pickable.worldName);
            }
            else
            {
                pickable = PickableFactory.GetMakeSpell(pickable.worldName);
            }
            pickable.GetPickable(player, true);
            player.gold = -pickable.GetGold();
            // TODO
            // menu.merchant.GetNode<Label>("s/v/label2").Text = $"Gold: {menu.player.gold.ToString("N0")}";
            Hide();
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
            itemList.RemoveItem(pickable, true, false, false);
            player.gold = pickable.GetGold();
            pickable.UnMake();
            // TODO
            // menu.merchant.GetNode<Label>("s/v/label2").Text = $"Gold: {menu.player.gold.ToString("N0")}";
            Hide();
        }
    }
}