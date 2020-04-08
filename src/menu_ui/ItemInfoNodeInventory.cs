using Godot;
using System;
using Game.Loot;
using Game.Database;
using Game.Actor;
namespace Game.Ui
{
    public class ItemInfoNodeInventory : ItemInfoNode 
    {
        [Signal]
        public delegate void ItemEquipped(Item item, bool on);

        public override void _Ready()
        {
            base._Ready();
            GetNode<BaseButton>("s/h/buttons/use")
                .Connect("pressed", this, nameof(_OnUsePressed));
            GetNode<BaseButton>("s/h/buttons/equip")
                .Connect("pressed", this, nameof(_OnEquipPressed));
            GetNode<BaseButton>("s/h/buttons/unequip")
                .Connect("pressed", this, nameof(_OnUnequipPressed));
            GetNode<BaseButton>("s/h/buttons/drop")
                .Connect("pressed", this, nameof(_OnDropPressed));
        }
        public void EquipItem(bool on)
        {
            ItemDB.ItemNode itemNode = ItemDB.GetItemData(pickableWorldName);
            Item item = PickableFactory.GetMakeItem(pickableWorldName);
            switch (itemNode.type)
            {
                case "WEAPON":
                    Tuple<int, int> values = item.GetValues();
                    player.weapon = (on) ? item : null;
                    player.minDamage += (on) ? values.Item1 : -values.Item1;
                    player.maxDamage += (on) ? values.Item2 : -values.Item2;
                    break;
                case "ARMOR":
                    player.vest = (on) ? item : null;
                    player.armor += (on) ? item.value : -item.value;
                    break;
            }
            if (on)
            {
                itemList.RemoveItem(pickableWorldName, true, false, false);
            }
            else
            {
                itemList.AddItem(pickableWorldName, false);
                ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
                foreach (ItemSlot otherItemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
                {
                    if (otherItemSlot.GetItem().Equals(pickableWorldName)
                    && !itemSlot.IsConnected(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut)))
                    {
                        itemSlot.Connect(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut));
                    }
                }
            }
            EmitSignal(nameof(ItemEquipped), item, on);
        }
        public void _OnUsePressed()
        {
            string itemType = ItemDB.GetItemData(pickableWorldName).type;
            string sndName = "click2";
            switch (itemType)
            {
                case "FOOD":
                    sndName = "eat";
                    if (player.state == Character.States.ATTACKING)
                    {
                        GetNode<Control>("s").Hide();
                        popup.GetNode<Label>("m/error/label").Text = "Cannot Eat\nIn Combat!";
                        popup.GetNode<Control>("m/error").Show();
                        popup.Show();
                        return;
                    }
                    break;
                case "POTION":
                    sndName = "drink";
                    break;
            }
            Globals.PlaySound(sndName, this, speaker);
            // TODO: going to need to get slot from inventory/spell bag
            // if (itemType.Equals("POTION"))
            // {
            //     itemList.SetSlotCoolDown(item.worldName, item.duration, 0.0f);
            // }
            // itemList.RemoveItem(pickableWorldName, true, false, false);
            // item.Consume(player, 0.0f);
            Hide();
        }
        public void _OnEquipPressed()
        {
            string itemType = ItemDB.GetItemData(pickableWorldName).type;
            if (!itemList.IsFull())
            {
                switch (itemType)
                {
                    case "WEAPON":
                        if (player.weapon != null)
                        {
                            player.weapon.Unequip();
                        }
                        break;
                    case "ARMOR":
                        if (player.vest != null)
                        {
                            player.vest.Unequip();
                        }
                        break;
                }
            }
            else if ((itemType.Equals("WEAPON") & player.weapon != null)
            || (itemType.Equals("ARMOR") & player.vest != null))
            {
                GetNode<Control>("s").Hide();
                popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
            else
            {
                ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
                itemSlot.SetBlockSignals(true);
                EquipItem(true);
                itemSlot.SetBlockSignals(false);
                Globals.PlaySound(ItemDB.GetItemData(pickableWorldName).material + "_off", this, speaker);
                Hide();    
            }
        }
        public void _OnUnequipPressed()
        {
            if (itemList.IsFull())
            {
                GetNode<Control>("s").Hide();
                popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
            else
            {
                Globals.PlaySound("inventory_unequip", this, speaker);
                EquipItem(false);
            }
        }
        public void _OnDropPressed()
        {
            RouteConnections(nameof(_OnDropConfirm));
            Globals.PlaySound("click2", this, speaker);
            GetNode<Control>("s").Hide();
            popup.GetNode<Label>("m/yes_no/label").Text = "Drop?";
            popup.GetNode<Control>("m/yes_no").Show();
            popup.Show();
        }
        public void _OnDropConfirm()
        {
            Globals.PlaySound("click2", this, speaker);
            itemList.RemoveItem(pickableWorldName);
            // TODO: hmmm...
            // player.GetNode("inventory").RemoveChild(pickable);
            // Globals.map.AddZChild(pickable);
            // pickable.Owner = Globals.map;
            // pickable.GlobalPosition = Globals.map.SetGetPickableLoc(player.GlobalPosition, true);
            // end
            GetNode<Control>("s").Show();
        }
    }
}