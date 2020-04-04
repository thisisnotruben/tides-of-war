using Godot;
using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Utils;
using Game.Loot;
using Game.Ability;
namespace Game.Ui
{
    public class MerchantNode : Control
    {
        public Player player;
        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                popup.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }        private Popup popup;
        private ItemList itemList;
        private ItemInfoNodeMerchant itemInfoNodeMerchant;
        // TODO set these:
        public ItemList spellBookItemList;
        public ItemList inventoryItemList;

        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            itemList = GetNode<ItemList>("s/v/c/merchant_list");
            itemInfoNodeMerchant = GetNode<ItemInfoNodeMerchant>("item_info_node");
            foreach (string nodePath in new string[] {"m/error/okay", "m/repair/back"})
            {
                popup.GetNode<BaseButton>(nodePath)
                    .Connect("pressed", this, nameof(_OnMerchantNodeHide));
            }
            popup.GetNode<BaseButton>("m/repair/repair_all")
                .Connect("pressed", this, nameof(_OnRepairConfirm),
                new Godot.Collections.Array() {"all"});
            popup.GetNode<BaseButton>("m/repair/repair_weapon")
                .Connect("pressed", this, nameof(_OnRepairConfirm),
                new Godot.Collections.Array() {"weapon"});
            popup.GetNode<BaseButton>("m/repair/repair_armor")
                .Connect("pressed", this, nameof(_OnRepairConfirm),
                new Godot.Collections.Array() {"armor"});
        }
        public void _OnMerchantNodeHide()
        {
            popup.Hide();
            GetNode<Control>("s").Show();
        }
        public void _OnMerchantIndexSelected(int itemIndex)
        {
            Pickable pickable = itemList.GetItemMetaData(itemIndex);
            bool isSpell = pickable is Spell;
            bool trained = false;
            Globals.PlaySound((isSpell) ? "spell_select"
                : Database.ItemDB.GetItemData(
                pickable.worldName).material + "_on", this, speaker);
            if (isSpell)
            {
                Globals.PlaySound("click1", this, speaker);
                List<Pickable> spells = spellBookItemList.GetItems(false);
                for (int i = 0; i < spells.Count && !trained; i++)
                {
                    trained = pickable.Equals(spells[i]);
                }
            }
            itemInfoNodeMerchant.Display(pickable, true,
                !GetNode<Label>("s/v/header").Text.Equals("Inventory"), trained);
        }
        public void _OnMerchantPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            itemList.Clear();
            GetNode<Label>("s/v/header").Text = player.target.worldName;
            GetNode<Control>("s/buttons/inventory").Show();
            GetNode<Control>("s/buttons/merchant").Hide();
            foreach (Node node in player.target.GetNode("inventory").GetChildren())
            {
                Pickable pickable = node as Pickable;
                if (pickable != null)
                {
                    pickable.SetUpShop(false);
                }
            }
        }
        public void _OnInventoryPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            itemList.Clear();
            GetNode<Label>("s/v/header").Text = "Inventory";
            GetNode<Control>("s/buttons/inventory").Hide();
            GetNode<Control>("s/buttons/merchant").Show();
            foreach (Pickable pickable in inventoryItemList.GetItems(true))
            {
                pickable.SetUpShop(true);
            }
        }
        public void _OnRepairPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            popup.GetNode<Control>("m/repair").Show();
            string text = "";
            if (player.weapon == null)
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Show();
                text = $"Weapon: {Stats.ItemRepairCost(player.weapon.level)}";
            }
            if (player.vest == null)
            {
                popup.GetNode<Control>("m/repair/repair_armor").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_armor").Show();
                int armorCost = Stats.ItemRepairCost(player.vest.level);
                text += (player.weapon == null) ? $"Armor: {armorCost}" : $"\nArmor: {armorCost}";
            }
            if (player.weapon != null && player.vest != null)
            {
                int total = Stats.ItemRepairCost(player.vest.level) + Stats.ItemRepairCost(player.weapon.level);
                text += $"\nAll: {total}";
            }
            popup.GetNode<Label>("m/repair/label").Text = text;
            popup.Show();
        }
        public void _OnRepairConfirm(string what)
        {
            Item weapon = player.weapon;
            Item armor = player.vest;
            int cost = 0;
            switch (what)
            {
                case "all":
                    cost = Stats.ItemRepairCost(weapon.level) + Stats.ItemRepairCost(armor.level);
                    break;
                case "weapon":
                    cost = Stats.ItemRepairCost(weapon.level);
                    break;
                case "armor":
                    cost = Stats.ItemRepairCost(armor.level);
                    break;
            }
            if (cost > player.gold)
            {
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
                popup.GetNode<Control>("m/repair").Hide();
                popup.GetNode<Control>("m/error").Show();
            }
            else
            {
                foreach (string sndName in new string[] {"sell_buy", "anvil"})
                {
                    Globals.PlaySound(sndName, this, speaker);    
                }
                player.gold = -cost;
                switch (what)
                {
                    case "all":
                        weapon.RepairItem(Item.MAX_DURABILITY);
                        armor.RepairItem(Item.MAX_DURABILITY);
                        break;
                    case "weapon":
                        weapon.RepairItem(Item.MAX_DURABILITY);
                        break;
                    case "armor":
                        armor.RepairItem(Item.MAX_DURABILITY);
                        break;
                }
                GetNode<Label>("s/v/label2").Text = $"Gold: {player.gold.ToString("N0")}";
                _OnMerchantNodeHide();
            }
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
            itemList.Clear();
        }
    }
}