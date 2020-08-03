using Godot;
using System.Collections.Generic;
using Game.Actor;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
    public class MerchantNode : GameMenu
    {
        public Npc merchant;
        private Popup popup;
        private ItemList itemList;
        private ItemInfoNodeMerchant itemInfoNodeMerchant;
        private ItemList _spellBookItemList;
        public ItemList spellBookItemList
        {
            set
            {
                _spellBookItemList = value;
                itemInfoNodeMerchant.spellBookItemList = value;
            }
            get
            {
                return _spellBookItemList;
            }
        }
        public ItemList inventoryItemList;
        public List<string> npcPickables;

        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            popup.Connect("hide", this, nameof(_OnMerchantNodeHide));
            itemList = GetNode<ItemList>("s/v/c/merchant_list");
            itemInfoNodeMerchant = GetNode<ItemInfoNodeMerchant>("item_info");
            itemInfoNodeMerchant.itemList = itemList;
            itemInfoNodeMerchant.Connect("hide", this, nameof(_OnMerchantNodeHide));
            itemInfoNodeMerchant.Connect(
                nameof(ItemInfoNodeMerchant.OnTransaction), this, nameof(_OnTransaction));
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
            npcPickables = new List<string>();
        }
        public void _OnTransaction(string pickableWorldName, int goldAmount, bool bought)
        {
            if (bought)
            {
                if (SpellDB.HasSpell(pickableWorldName))
                {
                    spellBookItemList.AddItem(pickableWorldName, true);
                }
                else
                {
                    inventoryItemList.AddItem(pickableWorldName, true);
                }
            }
            else
            {
                itemList.RemoveItem(pickableWorldName);
                if (SpellDB.HasSpell(pickableWorldName))
                {
                    spellBookItemList.RemoveItem(pickableWorldName);
                }
                else
                {
                    inventoryItemList.RemoveItem(pickableWorldName);
                }
            }
            player.gold += goldAmount;
            GetNode<Label>("s/v/sub_header").Text = "Gold: " + player.gold;
        }
        public void DisplayItems(string header, params string[] pickableWorldNames)
        {
            GetNode<Label>("s/v/header").Text = header;
            itemInfoNodeMerchant.isBuying = !header.Equals("Inventory");
            Pickable pickable;
            foreach (string worldName in pickableWorldNames)
            {
                if (SpellDB.HasSpell(worldName))
                {
                    pickable = PickableFactory.GetMakeSpell(worldName);
                }
                else
                {
                    pickable = PickableFactory.GetMakeItem(worldName);
                }
                itemList.AddItem(pickable.worldName, true);
            }
        }
        public void _OnMerchantNodeDraw()
        {
            Globals.PlaySound(
                (npcPickables.Count > 0 &&  SpellDB.HasSpell(npcPickables[0]))
                ? "turn_page" : "merchant_open", this, speaker);
            GetNode<Label>("s/v/sub_header").Text = "Gold: " + player.gold;
        }
        public void _OnMerchantNodeHide()
        {
            if (!Visible)
            {
                Globals.PlaySound("merchant_close", this, speaker);    
            }
            popup.Hide();
            GetNode<Control>("s").Show();
        }
        public void _OnMerchantIndexSelected(int itemIndex)
        {
            string pickableWorldName = itemList.GetItemMetaData(itemIndex);
            bool isSpell = SpellDB.HasSpell(pickableWorldName);
            bool alreadyHave = false;
            Globals.PlaySound((isSpell) ? "spell_select"
                : Database.ItemDB.GetItemData(
                pickableWorldName).material + "_on", this, speaker);
            if (isSpell)
            {
                Globals.PlaySound("click1", this, speaker);
                alreadyHave = spellBookItemList.HasItem(pickableWorldName, false);
            }
            GetNode<Control>("s").Hide();
            itemInfoNodeMerchant.Display(pickableWorldName, true,
                !GetNode<Label>("s/v/header").Text.Equals("Inventory"), alreadyHave);
        }
        public void _OnMerchantPressed()
        {
            
            Globals.PlaySound("click1", this, speaker);
            GetNode<Control>("s/buttons/inventory").Show();
            GetNode<Control>("s/buttons/merchant").Hide();
            itemList.Clear();
            DisplayItems(merchant.worldName,
                ContentDB.GetContentData(merchant.Name).merchandise.ToArray());
        }
        public void _OnInventoryPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            GetNode<Control>("s/buttons/inventory").Hide();
            GetNode<Control>("s/buttons/merchant").Show();
            itemList.Clear();
            DisplayItems("Inventory", inventoryItemList.GetItems(false).ToArray());
        }
        public void _OnRepairPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            popup.GetNode<Control>("m/repair").Show();
            string text = "";
            Item playerWeapon = player.weapon;
            Item playerArmor = player.vest;
            int weaponLevel = (playerWeapon == null) ? 0 : PickableDB.GetLevel(playerWeapon.worldName);
            int armorLevel = (playerArmor == null) ? 0 : PickableDB.GetLevel(playerArmor.worldName);
            if (playerWeapon == null)
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Show();
                text = $"Weapon: {Stats.ItemRepairCost(weaponLevel)}";
            }
            if (playerArmor == null)
            {
                popup.GetNode<Control>("m/repair/repair_armor").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_armor").Show();
                int armorCost = Stats.ItemRepairCost(armorLevel);
                text += (playerWeapon == null) ? $"Armor: {armorCost}" : $"\nArmor: {armorCost}";
            }
            if (playerWeapon!= null && playerArmor != null)
            {
                int total = Stats.ItemRepairCost(armorLevel) + Stats.ItemRepairCost(weaponLevel);
                text += $"\nAll: {total}";
            }
            popup.GetNode<Label>("m/repair/label").Text = text;
            popup.Show();
        }
        public void _OnRepairConfirm(string what)
        {
            Item weapon = player.weapon;
            Item armor = player.vest;
            int weaponLevel = ItemDB.GetItemData(weapon.worldName).level;
            int armorLevel = ItemDB.GetItemData(armor.worldName).level;
            int cost = 0;
            switch (what)
            {
                case "all":
                    cost = Stats.ItemRepairCost(weaponLevel) + Stats.ItemRepairCost(armorLevel);
                    break;
                case "weapon":
                    cost = Stats.ItemRepairCost(weaponLevel);
                    break;
                case "armor":
                    cost = Stats.ItemRepairCost(armorLevel);
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
        public override void _OnBackPressed()
        {
            base._OnBackPressed();
            itemList.Clear();
        }
    }
}