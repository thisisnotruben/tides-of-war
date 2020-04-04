using Godot;
using System;
using Game.Loot;
using Game.Database;
using Game.Actor;
namespace Game.Ui
{
    public class ItemInfoNodeInventory : ItemInfoNode 
    {
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
        public void EquipItem(Item item, bool on)
        {
            switch (item.worldType)
            {
                case WorldObject.WorldTypes.WEAPON:
                    Tuple<int, int> values = item.GetValues();
                    player.weapon = (on) ? item : null;
                    player.minDamage += (on) ? values.Item1 : -values.Item1;
                    player.maxDamage += (on) ? values.Item2 : -values.Item2;
                    break;
                case WorldObject.WorldTypes.ARMOR:
                    player.vest = (on) ? item : null;
                    player.armor += (on) ? item.value : -item.value;
                    break;
            }
            if (on)
            {
                itemList.RemoveItem(item, true, false, false);
            }
            else
            {
                itemList.AddItem(item, false);
                ItemSlot itemSlot = itemList.GetItemSlot(item);
                foreach (ItemSlot otherItemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
                {
                    if (otherItemSlot.GetItem() == item
                    && !itemSlot.IsConnected(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut)))
                    {
                        itemSlot.Connect(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut));
                    }
                }
            }
        }
        public void _OnUsePressed()
        {
            string sndName = "click2";
            switch (pickable.worldType)
            {
                case WorldObject.WorldTypes.FOOD:
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
                case WorldObject.WorldTypes.POTION:
                    sndName = "drink";
                    break;
            }
            Globals.PlaySound(sndName, this, speaker);
            Item item = pickable as Item;
            if (pickable.worldType == WorldObject.WorldTypes.POTION)
            {
                itemList.SetSlotCoolDown(item, item.duration, 0.0f);
            }
            itemList.RemoveItem(item, true, false, false);
            item.Consume(player, 0.0f);
            Hide();
        }
        public void _OnEquipPressed()
        {
            if (!itemList.IsFull())
            {
                switch (pickable.worldType)
                {
                    case WorldObject.WorldTypes.WEAPON:
                        if (player.weapon != null)
                        {
                            player.weapon.Unequip();
                        }
                        break;
                    case WorldObject.WorldTypes.ARMOR:
                        if (player.vest != null)
                        {
                            player.vest.Unequip();
                        }
                        break;
                }
            }
            else if ((pickable.worldType == WorldObject.WorldTypes.WEAPON & player.weapon != null)
            || (pickable.worldType == WorldObject.WorldTypes.ARMOR & player.vest != null))
            {
                GetNode<Control>("s").Hide();
                popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
            else
            {
                ItemSlot itemSlot = itemList.GetItemSlot(pickable);
                itemSlot.SetBlockSignals(true);
                EquipItem(pickable as Item, true);
                itemSlot.SetBlockSignals(false);
                Globals.PlaySound(ItemDB.GetItemData(pickable.worldName).material + "_off", this, speaker);
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
                EquipItem(pickable as Item, false);
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
            pickable.Drop();
            itemList.RemoveItem(pickable);
            // TODO: hmmm...
            player.GetNode("inventory").RemoveChild(pickable);
            Globals.map.AddZChild(pickable);
            pickable.Owner = Globals.map;
            pickable.GlobalPosition = Globals.map.SetGetPickableLoc(player.GlobalPosition, true);
            // end
            GetNode<Control>("s").Show();
        }
    }
}