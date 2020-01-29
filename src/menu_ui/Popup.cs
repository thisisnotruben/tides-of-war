using System;
using System.Collections.Generic;
using Game.Misc.Loot;
using Godot;
namespace Game.Ui
{
    public class Popup : Control
    {
        private Menu menu;
        public Control about;
        public override void _Ready()
        {
            menu = Owner as  Menu;
        }
        public void _OnDeletePressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            GetNode<Control>("m/save_load").Hide();
            GetNode<Label>("m/yes_no/label").Text = "Delete?";
            GetNode<Control>("m/yes_no").Show();
            Show();
        }
        public void _OnPopupHide()
        {
            if (menu is InGameMenu)
            {
                menu.GetNode<ColorRect>("c/game_menu/bg").Color = new Color("#6e6e6e");
            }
            ((Control)menu.listOfMenus.GetParent()).Show();
            foreach (Control control in GetNode("m").GetChildren())
            {
                control.Hide();
            }
        }
        public void _OnPopupDraw()
        {
            if (menu is InGameMenu)
            {
                menu.GetNode<ColorRect>("c/game_menu/bg").Color = new Color(0.0f, 0.0f, 0.0f, 0.25f);
                ((Control)menu.listOfMenus.GetParent()).Hide();
            }
        }
        public void _OnErrorDraw()
        {
            Globals.PlaySound("click6", this, menu.snd);
        }
        public void _OnMResized()
        {
            GetNode<Control>("bg").RectMinSize = GetNode<Control>("m").RectSize;
        }
        public void _OnAllPressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            foreach (QuestEntry questSlot in menu.questLog.GetNode("s/v/s/v").GetChildren())
            {
                questSlot.Show();
            }
            Hide();
            menu.questLog.Show();
        }
        public void _OnActivePressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            foreach (QuestEntry questSlot in menu.questLog.GetNode("s/v/s/v").GetChildren())
            {
                if (questSlot.GetQuest().GetState() != Game.Quests.WorldQuests.QuestState.ACTIVE)
                {
                    questSlot.Hide();
                }
                else
                {
                    questSlot.Show();
                }
            }
            Hide();
            menu.questLog.Show();
        }
        public void _OnCompletedPressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            foreach (QuestEntry questSlot in menu.questLog.GetNode("s/v/s/v").GetChildren())
            {
                if (questSlot.GetQuest().GetState() != Game.Quests.WorldQuests.QuestState.DELIVERED)
                {
                    questSlot.Hide();
                }
                else
                {
                    questSlot.Show();
                }
            }
            Hide();
            menu.questLog.Show();
        }
        public void _OnBackPressed()
        {
            bool playSnd = false;
            if (menu is StartMenu)
            {
                ((Control)menu.listOfMenus.GetParent()).Show();
                menu.saveLoad.Show();
            }
            else if (GetNode<Control>("m/exit").Visible)
            {
                menu.menu.Show();
            }
            else if (GetNode<Control>("m/filter_options").Visible)
            {
                menu.questLog.Show();
            }
            else if (GetNode<Control>("m/add_to_slot").Visible)
            {
                menu.itemInfo.Show();
                menu.GetNode<Control>("c/controls/right").Show();
                if (menu is InGameMenu && menu.itemInfo.GetNode("s/h/v/back").IsConnected("pressed", menu, nameof(InGameMenu.HideMenu)))
                {
                    playSnd = true;
                }
                else if (GetNode<Control>("m/add_to_slot/clear_slot").Visible)
                {
                    Globals.PlaySound("click1", this, menu.snd);
                    playSnd = true;
                }
                foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
                {
                    Tween tween = itemSlot.GetNode<Tween>("tween");
                    tween.SetActive(true);
                    tween.ResumeAll();
                    itemSlot.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
                    itemSlot.GetNode<Control>("m/label").Hide();
                }
            }
            else if (!GetNode<Control>("m/repair").Visible)
            {
                menu.saveLoad.Show();
            }
            if (!playSnd)
            {
                Globals.PlaySound("click3", this, menu.snd);
            }
            Hide();
        }
        public void _OnExitGamePressed()
        {
            GetTree().Quit();
        }
        public void _OnExitMenuPressed()
        {
            Globals.PlaySound("click0", this, menu.snd);
            GetTree().Paused = false;
            Globals.SetScene("res://src/menu_ui/StartMenu.tscn", GetTree().Root, Globals.GetMap());
            Globals.GetWorldQuests().Reset();
        }
        public void _OnOkayPressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            if (menu.player.GetTarget() != null && menu.player.GetTarget().GetWorldType() == WorldObject.WorldTypes.MERCHANT &&
                menu.selected == null && GetNode<Label>("m/error/label").Text.Equals("Not Enough\nGold!"))
            {
                GetNode<Label>("m/error").Hide();
                GetNode<Control>("m/repair").Show();
            }
            else
            {
                Hide();
                if (menu.dialogue.IsVisibleInTree())
                {
                    menu.dialogue.Show();
                }
                else if (menu is InGameMenu && menu.selected == null)
                {
                    ((InGameMenu)menu).HideMenu();
                }
                else
                {
                    menu.itemInfo.Show();
                    menu.listOfMenus.Show();
                }
            }
        }
        public void _OnYesPressed()
        {
            if (menu is StartMenu)
            {
                Globals.SaveGameData("", menu.selectedIdx);
                menu.saveLoad.GetNode<Label>($"v/s/c/g/slot_label_{menu.selectedIdx}").Text = $"Slot {menu.selectedIdx + 1}";
                new Directory().Remove(Globals.SAVE_PATH[$"SAVE_SLOT_{menu.selectedIdx}"]);
                _OnNoPressed();
            }
            else
            {
                string sndName = "click1";
                Hide();
                switch (GetNode<Label>("m/yes_no/label").Text)
                {
                    case "Drop?":
                        sndName = "inventory_drop";
                        ((Pickable)menu.selected).Drop();
                        if (menu.itemInfo.GetNode<Control>("s/h/left").Visible)
                        {
                            menu.inventory.Show();
                        }
                        else if (menu is InGameMenu)
                        {
                            ((InGameMenu)menu).HideMenu();
                        }
                        break;
                    case "Unequip?":
                        sndName = "inventory_unequip";
                        ((Item)menu.selected).Unequip();
                        Texture texture = (Texture)GD.Load("res://asset/img/ui/black_bg_icon.tres");
                        string nodePath = $"s/v/h/{Enum.GetName(typeof(WorldObject.WorldTypes), ((Item)menu.selected).GetWorldType()).ToLower()}_slot";
                        menu.inventory.GetNode<TextureButton>(nodePath).TextureNormal = texture;
                        menu.statsMenu.GetNode<TextureButton>(nodePath).TextureNormal = texture;
                        if (menu is InGameMenu && menu.itemInfo.GetNode("s/h/v/back").IsConnected("pressed", menu, nameof(InGameMenu.HideMenu)))
                        {
                            ((InGameMenu)menu).HideMenu();
                        }
                        else
                        {
                            menu.inventory.Show();
                        }
                        break;
                    case "Buy?":
                    case "Learn?":
                        if (GetNode<Label>("m/yes_no/label").Text.Equals("Learn?"))
                        {
                            sndName = "learn_spell";
                        }
                        Globals.PlaySound("sell_buy", this, menu.snd);
                        ((Pickable)menu.selected).Buy(menu.player);
                        menu.merchant.GetNode<Label>("s/v/label2").Text = 
                            $"Gold: {menu.player.GetGold().ToString("N0")}";
                        menu.merchant.Show();
                        break;
                    case "Delete?":
                        Globals.SaveGameData("", menu.selectedIdx);
                        ((Label)menu.selected).Text = $"Slot {menu.selectedIdx + 1}";
                        new Directory().Remove(Globals.SAVE_PATH[$"SAVE_SLOT_{menu.selectedIdx}"]);
                        menu.saveLoad.Show();
                        break;
                    case "Sell?":
                        sndName = "sell_buy";
                        menu.merchantBag.RemoveItem(menu.selectedIdx, true, false, false);
                        menu.inventoryBag.RemoveItem(menu.selectedIdx, true, false, false);
                        ((Pickable)menu.selected).Sell(menu.player);
                        menu.merchant.GetNode<Label>("s/v/label2").Text = $"Gold: {menu.player.GetGold().ToString("N0")}";
                        menu.merchant.Show();
                        break;
                    case "Overwrite?":
                        SaveGame();
                        menu.saveLoad.Show();
                        break;
                }
                Globals.PlaySound(sndName, this, menu.snd);
                menu.selectedIdx = -1;
                menu.selected = null;
            }
        }
        public void _OnNoPressed()
        {
            Globals.PlaySound("click3", this, menu.snd);
            Hide();
            if (menu is StartMenu)
            {
                ((Control)menu.listOfMenus.GetParent()).Show();
                menu.listOfMenus.Show();
            }
            else
            {
                switch (GetNode<Label>("m/yes_no/label").Text)
                {
                    case "Delete?":
                        menu.selectedIdx = 1;
                        menu.selected = null;
                        menu.saveLoad.Show();
                        break;
                    case "Overwrite?":
                        menu.saveLoad.Show();
                        break;
                    default:
                        menu.itemInfo.Show();
                        break;
                }
            }
        }
        public void _OnSavePressed()
        {
            Globals.PlaySound("click1", this, menu.snd);
            if (((Label)menu.selected).Text.Contains("Slot"))
            {
                Hide();
                SaveGame();
                menu.selectedIdx = -1;
                menu.selected = null;
                menu.saveLoad.Show();
            }
            else
            {
                GetNode<Label>("m/yes_no/label").Text = "Overwrite?";
                GetNode<Control>("m/save_load").Hide();
                GetNode<Control>("m/yes_no").Show();
            }
        }
        public void _OnRepairPressed(string what)
        {
            Item weapon = menu.player.GetWeapon();
            Item armor = menu.player.GetArmor();
            int cost = 0;
            switch (what)
            {
                case "all":
                    cost = Stats.ItemRepairCost(weapon.GetLevel()) + Stats.ItemRepairCost(armor.GetLevel());
                    break;
                case "weapon":
                    cost = Stats.ItemRepairCost(weapon.GetLevel());
                    break;
                case "armor":
                    cost = Stats.ItemRepairCost(armor.GetLevel());
                    break;
            }
            if (cost > menu.player.GetGold())
            {
                GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
                GetNode<Control>("m/repair").Hide();
                GetNode<Control>("m/error").Show();
            }
            else
            {
                Globals.PlaySound("sell_buy", this, menu.snd);
                Globals.PlaySound("anvil", this, menu.snd);
                menu.player.SetGold((short) - cost);
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
                Hide();
                menu.merchant.GetNode<Label>("s/v/label2").Text = $"Gold: {menu.player.GetGold().ToString("N0")}";
                menu.merchant.Show();
            }
        }
        public void _OnLoadPressed()
        {
            Globals.PlaySound("click0", this, menu.snd);
            Globals.LoadGame(Globals.SAVE_PATH[$"SAVE_SLOT_{menu.selected}"]);
            if (menu is InGameMenu)
            {
                menu.GetNode<Control>("c/game_menu").Hide();
            }
        }
        public void _OnSlotPressed(int index)
        {
            int amounttt = -1;
            Pickable selectedPickable = (Pickable)menu.selected;
            ItemSlot buttonFrom = null;
            ItemSlot buttonTo = null;
            Hide();
            Globals.PlaySound("click1", this, menu.snd);
            menu.GetNode<Control>("c/controls/right").Show();
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween itemSlotTween = itemSlot.GetNode<Tween>("tween");
                itemSlot.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
                itemSlot.GetNode<Control>("m/label").Hide();
                itemSlotTween.SetActive(true);
                itemSlotTween.ResumeAll();
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(selectedPickable))
                {
                    amounttt = itemSlot.GetItemStack().Count;
                    itemSlot.SetItem(null, false, true, false);
                }
                if (itemSlot.Name.Equals(index.ToString()))
                {
                    buttonTo = itemSlot;
                    if (itemSlot.GetItem() != null)
                    {
                        itemSlot.SetItem(null, false, true, false);
                    }
                    Item weapon = menu.player.GetWeapon();
                    Item armor = menu.player.GetArmor();
                    if (weapon == menu.selected)
                    {
                        itemSlot.SetItem(weapon, false, false, false);
                    }
                    else if (armor == menu.selected)
                    {
                        itemSlot.SetItem(armor, false, false, false);
                    }
                    else
                    {
                        foreach (ItemList itemList in new ItemList[] { menu.inventoryBag, menu.spellBook })
                        {
                            if (itemList.HasItem(selectedPickable))
                            {
                                ItemSlot bagItemSlot = itemList.GetItemSlot(selectedPickable);
                                List<Pickable> pickableStack = bagItemSlot.GetItemStack();
                                buttonFrom = bagItemSlot;
                                if (amounttt == -1)
                                {
                                    amounttt = pickableStack.Count;
                                }
                                for (short i = 0; i < amounttt; i++)
                                {
                                    itemSlot.SetItem(pickableStack[i]);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            if (buttonFrom != null && buttonTo != null)
            {
                foreach (Godot.Collections.Dictionary link in buttonFrom.GetSignalConnectionList(nameof(ItemSlot.SyncSlot)))
                {
                    buttonFrom.Disconnect(nameof(ItemSlot.SyncSlot), (Godot.Object)link["target"], nameof(ItemSlot._OnSyncShortcut));
                }
                buttonFrom.Connect(nameof(ItemSlot.SyncSlot), buttonTo, nameof(ItemSlot._OnSyncShortcut));
                if (buttonFrom.IsCoolingDown())
                {
                    buttonTo.CoolDown(buttonFrom.GetItem(), buttonFrom.GetCoolDownInitialTime(), buttonFrom.GetCoolDownTimeLeft());
                }
            }
        }
        public void _OnClearSlotPressed()
        {
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals((Pickable)menu.selected))
                {
                    itemSlot.SetItem(null, false, true, false);
                }
            }
            if (GetTree().Paused)
            {
                _OnBackPressed();
            }
            if (menu is InGameMenu && menu.itemInfo.GetNode("s/h/v/back")
            .IsConnected("pressed", menu, nameof(InGameMenu.HideMenu)))
            {
                ((InGameMenu)menu).HideMenu();
            }
        }
        public void _OnRepairDraw()
        {
            short shown = 0;
            foreach (Control node in GetNode("m/repair").GetChildren())
            {
                if (node.Visible)
                {
                    shown++;
                }
            }
            if (shown > 4)
            {
                GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey2_bg.tres");
            }
        }
        public void _OnRepairHide()
        {
            GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey3_bg.tres");
        }
        public void SaveLoadGo(int index)
        {
            if (menu is StartMenu)
            {
                if (!menu.saveLoad.GetNode<Label>($"v/s/c/g/slot_label_{index}").Text.Equals($"Slot {index + 1}"))
                {
                    Globals.PlaySound("click2", this, menu.snd);
                    ((Control)menu.listOfMenus.GetParent()).Hide();
                    GetNode<Control>("m/save_load/save").Hide();
                }
                else
                {
                    return;
                }
            }
            else
            {
                Globals.PlaySound("click2", this, menu.snd);
                menu.saveLoad.Hide();
                GetNode<Control>("m/save_load/save").Show();
                menu.selected = menu.saveLoad.GetNode<Label>($"v/s/c/g/slot_label_{index}");
                Control load = GetNode<Control>("m/save_load/load");
                Control delete = GetNode<Control>("m/save_load/delete");
                if (((Label)menu.selected).Text.Equals($"Slot {index + 1}"))
                {
                    load.Hide();
                    delete.Hide();
                }
                else
                {
                    load.Show();
                    delete.Show();
                }
            }
            menu.selectedIdx = index;
            GetNode<Control>("m/save_load").Show();
            Show();
        }
        private void SaveGame()
        {
            Godot.Collections.Dictionary date = OS.GetDatetime();
            string time = $"{date["month"]}-{date["day"]} {date["hour"]}:{date["minute"]}";
            ((Label)menu.selected).Text = time;
            Globals.SaveGameData(time, menu.selectedIdx);
            Globals.SaveGame(Globals.SAVE_PATH[$"SAVE_SLOT_{menu.selectedIdx}"]);
            ((Label)menu.selected).Text = time;
            menu.saveLoad.SetLabels();
        }
    }
}