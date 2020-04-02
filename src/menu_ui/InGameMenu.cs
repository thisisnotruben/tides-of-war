using System;
using System.Collections.Generic;
using System.Linq;
using Game.Ability;
using Game.Actor;
using Game.Database;
using Game.Loot;
using Game.Actor.Doodads;
using Game.Quests;
using Game.Utils;
using Godot;
namespace Game.Ui
{
    public class InGameMenu : Menu
    {
        private static readonly PackedScene questEntryScene = (PackedScene)GD.Load("res://src/menu_ui/quest_entry.tscn");
        public Control spellMenu;
        public Control hpMana;
        public enum Bags : byte { INVENTORY, MERCHANT, SPELL }
        public override void _Ready()
        {
            listOfMenus = GetNode<Control>("c/game_menu/list_of_menus/m");
            menu = listOfMenus.GetNode<Control>("main_menu");
            inventory = listOfMenus.GetNode<Control>("inventory");
            merchant = listOfMenus.GetNode<Control>("merchant");
            statsMenu = listOfMenus.GetNode<Control>("stats");
            questLog = listOfMenus.GetNode<Control>("quest_log");
            saveLoad = listOfMenus.GetNode<SaveLoad>("save_load");
            itemInfo = listOfMenus.GetNode<Control>("item_info");
            dialogue = listOfMenus.GetNode<Control>("dialogue");
            spellMenu = listOfMenus.GetNode<Control>("spell_book");
            about = listOfMenus.GetNode<Control>("about");
            inventoryBag = inventory.GetNode<ItemList>("s/v/c/item_list");
            merchantBag = merchant.GetNode<ItemList>("s/v/c/merchant_list");
            spellBook = spellMenu.GetNode<ItemList>("s/v/c/spell_list");
            popup = GetNode<Popup>("c/game_menu/popup");
            hpMana = GetNode<Control>("c/hp_mana");
            snd = GetNode<Speaker>("snd");
            saveLoad.GetNode("v/s/back").Connect("pressed", this, nameof(_OnBackPressed));
            player = (Player)Owner;
            merchantBag.allowSlotsToCooldown = false;
            spellBook.ITEM_MAX = 30;
            foreach (Node container in new Node[] { hpMana.GetNode("m/h/p/h/g"), hpMana.GetNode("m/h/u/h/g") })
            {
                foreach (Node node in container.GetChildren())
                {
                    ItemSlot itemSlot = node as ItemSlot;
                    if (itemSlot != null)
                    {
                        itemSlot.slotType = ItemSlot.SlotType.HUD_SLOT;
                    }
                }
            }
            foreach (Node control in new Node[] { inventoryBag, spellBook })
            {
                foreach (Node node in control.GetChildren())
                {
                    ItemSlot itemSlot = node as ItemSlot;
                    if (itemSlot != null)
                    {
                        foreach (Node hudNode in hpMana.GetNode("m/h/p/h/g").GetChildren())
                        {
                            ItemSlot hudSlot = hudNode as ItemSlot;
                            if (hudSlot != null)
                            {
                                itemSlot.Connect(nameof(ItemSlot.Cooldown), hudSlot, nameof(ItemSlot.CoolDown));
                                hudSlot.Connect(nameof(ItemSlot.Cooldown), itemSlot, nameof(ItemSlot.CoolDown));
                            }
                        }
                        foreach (Node hudNode in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
                        {
                            ItemSlot hudSlot = hudNode as ItemSlot;
                            if (hudSlot != null)
                            {
                                hudSlot.slotType = ItemSlot.SlotType.SHORTCUT;
                                itemSlot.Connect(nameof(ItemSlot.Cooldown), hudSlot, nameof(ItemSlot.CoolDown));
                                hudSlot.Connect(nameof(ItemSlot.Cooldown), itemSlot, nameof(ItemSlot.CoolDown));
                                if (!hudSlot.IsConnected(nameof(ItemSlot.ShortcutPressed), this, nameof(_OnHudSlotPressed)))
                                {
                                    hudSlot.Connect(nameof(ItemSlot.ShortcutPressed), this, nameof(_OnHudSlotPressed));
                                }
                            }
                        }
                    }
                }
            }
            foreach (Node container in new Node[] { itemInfo.GetNode("s/v/c/v/c/label"), questLog.GetNode("s/v/s") })
            {
                foreach (Node node in container.GetChildren())
                {
                    ScrollBar scrollBar = node as ScrollBar;
                    if (scrollBar != null)
                    {
                        scrollBar.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    }
                }
            }
        }
        public void _OnResumePressed()
        {
            Globals.PlaySound("click2", this, snd);
            player.SetBuff();
            HideMenu();
        }
        public void _OnInventoryPressed()
        {
            Globals.PlaySound("click1", this, snd);
            if (merchant.Visible)
            {
                merchantBag.Clear();
                merchant.GetNode<Label>("s/v/label").Text = "Inventory";
                merchant.GetNode<Control>("s/v2/inventory").Hide();
                merchant.GetNode<Control>("s/v2/merchant").Show();
                foreach (Pickable pickable in inventoryBag.GetItems(true))
                {
                    pickable.SetUpShop(true);
                }
            }
            else
            {
                menu.Hide();
                inventory.Show();
            }
        }
        public void _OnMerchantPressed()
        {
            Globals.PlaySound("click1", this, snd);
            merchantBag.Clear();
            merchant.GetNode<Label>("s/v/label").Text = player.target.worldName;
            merchant.GetNode<Control>("s/v2/merchant").Hide();
            merchant.GetNode<Control>("s/v2/inventory").Show();
            foreach (Node node in player.target.GetNode("inventory").GetChildren())
            {
                Pickable pickable = node as Pickable;
                if (pickable != null)
                {
                    pickable.SetUpShop(false);
                }
            }
        }
        public void _OnStatsPressed()
        {
            Globals.PlaySound("click1", this, snd);
            string statsDescription = $"Name: {player.worldName}\nHealth: {player.hp} / {player.hpMax}\n" +
                $"Mana: {player.mana} / {player.manaMax}\nXP: {player.xp}\nLevel: {player.level}\n" +
                $"Gold: {player.gold.ToString("N0")}\nStamina: {player.stamina}\nIntellect: {player.intellect}\n" +
                $"Agility: {player.agility}\nArmor: {player.armor}\nDamage: {player.minDamage} - {player.maxDamage}\n" +
                $"Attack Speed: {player.weaponSpeed.ToString("0.00")}\nAttack Range: {player.weaponRange}";
            statsMenu.GetNode<RichTextLabel>("s/v/c/label").BbcodeText = statsDescription;
            menu.Hide();
            statsMenu.Show();
        }
        public void _OnQuestLogPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            questLog.Show();
        }
        public void _OnAboutPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            about.Show();
        }
        public void _OnSaveLoadPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            saveLoad.Show();
        }
        public void _OnExitPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            popup.GetNode<Control>("m/exit").Show();
            popup.Show();
        }
        public void _OnPausePressed(bool slotSelect)
        {
            GetNode<Control>("c/game_menu").Show();
            if (!slotSelect)
            {
                Globals.PlaySound("click5", this, snd);
                menu.Show();
            }
        }
        public void _OnSpellBookPressed()
        {
            Globals.PlaySound("turn_page", this, snd);
            spellMenu.GetNode<Label>("s/v/m/v/label").Text = $"Health: {player.hp} / {player.hpMax}";
            spellMenu.GetNode<Label>("s/v/m/v/label2").Text = $"Mana: {player.mana} / {player.manaMax}";
            menu.Hide();
            GetNode<Control>("c/game_menu").Show();
            spellMenu.Show();
        }
        public void _OnMiniMapPressed()
        {
            Sprite miniMap = GetNode<Sprite>("c/mini_map/map");
            if (miniMap.Texture != null)
            {
                Globals.PlaySound("click5", this, snd);
                if (player.spell != null &&
                    player.spell.subType == WorldObject.WorldTypes.CHOOSE_AREA_EFFECT)
                {
                    player.spell.UnMake();
                }
                else if (miniMap.Visible)
                {
                    miniMap.Hide();
                }
                else
                {
                    miniMap.Show();
                }
            }
        }
        public void _OnUnitHudPressed()
        {
            if (player.target != null)
            {
                Globals.PlaySound("click5", this, snd);
                player.target = null;
            }
        }
        public void _OnBackPressed()
        {
            string sndName = "click3";
            if (inventory.Visible)
            {
                inventory.Hide();
                menu.Show();
            }
            else if (statsMenu.Visible)
            {
                statsMenu.Hide();
                menu.Show();
            }
            else if (questLog.Visible)
            {
                foreach (Control control in questLog.GetNode("s/v/s/v").GetChildren())
                {
                    control.Show();
                }
                questLog.Hide();
                menu.Show();
            }
            else if (saveLoad.Visible)
            {
                saveLoad.Hide();
                menu.Show();
            }
            else if (spellMenu.Visible)
            {
                sndName = "spell_book_close";
                spellMenu.Hide();
                HideMenu();
            }
            else if (itemInfo.Visible)
            {
                itemInfo.Hide();
                Pickable selectedPickable = selected as Pickable;
                if (selectedPickable != null)
                {
                    if (spellBook.HasItem(selectedPickable))
                    {
                        spellMenu.Show();
                    }
                    else
                    {
                        itemInfo.GetNode<Control>("s/h/left").Show();
                        itemInfo.GetNode<Control>("s/h/right").Show();
                        sndName = SndConfigure();
                        if (!player.GetNode("inventory").GetChildren().Contains(selectedPickable) ||
                            merchant.GetNode<Label>("s/v/label").Text.Equals("Inventory"))
                        {
                            sndName = sndName.Replace("on", "off");
                            merchant.Show();
                        }
                        else if (selectedIdx == -2)
                        {
                            statsMenu.Show();
                        }
                        else
                        {
                            inventory.Show();
                        }
                    }
                }
                else
                {
                    GD.Print("Unexpected type in method _OnBackPressed on condition itemInfo.Visible");
                }
                selected = null;
                selectedIdx = -1;
            }
            else if (dialogue.Visible)
            {
                dialogue.Hide();
                if (selected == questLog)
                {
                    questLog.Show();
                    selected = null;
                }
                else
                {
                    dialogue.GetNode<Control>("s/s/v/accept").Hide();
                    dialogue.GetNode<Control>("s/s/v/finish").Hide();
                    hpMana.GetNode<Control>("m/h/u").Hide();
                    player.target = null;
                    HideMenu();
                }
            }
            else if (merchant.Visible)
            {
                if (player.target != null)
                {
                    if (player.target.worldType == WorldObject.WorldTypes.TRAINER)
                    {
                        itemInfo.GetNode<Label>("s/h/v/buy/label").Text = "Buy";
                        popup.GetNode<Label>("m/yes_no/label").Text = "Buy?";
                        sndName = "spell_book_close";
                    }
                    else
                    {
                        sndName = "merchant_close";
                    }
                }
                itemInfo.GetNode<TextureButton>("s/v/c/v/bg").Disabled = false;
                merchant.GetNode<Label>("s/v/label").Text = "";
                merchant.GetNode<Control>("s/v2/merchant").Hide();
                merchant.GetNode<Control>("s/v2/repair").Hide();
                merchant.GetNode<Control>("s/v2/inventory").Show();
                merchant.Hide();
                merchantBag.Clear();
                player.target = null;
                HideMenu();
            }
            Globals.PlaySound(sndName, this, snd);
        }
        public void _OnSpellSelected(int idx, bool sift)
        {
            if (!sift)
            {
                Globals.PlaySound("spell_select", this, snd);
            }
            selected = spellBook.GetItemMetaData(idx);
            Pickable selectedPickable = (Pickable)selected;
            selectedIdx = idx;
            ItemInfoHideExcept((spellBook.IsSlotCoolingDown(idx) || player.dead) ? "" : "cast");
            selectedPickable.Describe();
            if (selectedPickable.GetIndex() == 0)
            {
                itemInfo.GetNode<TextureButton>("s/h/left").Disabled = true;
            }
            if (spellBook.GetItemCount() - 1 == idx)
            {
                itemInfo.GetNode<TextureButton>("s/h/right").Disabled = true;
            }
            spellMenu.Hide();
            itemInfo.Show();
        }
        public void _OnBagIndexSelected(int idx, bool sift = false)
        {
            ItemInfoHideExcept();
            selectedIdx = idx;
            ItemList bag = inventoryBag;
            Pickable selectedPickable;
            string sndName;
            if (merchantBag.GetItemCount() == 0)
            {
                sndName = "inventory_open";
                inventory.Hide();
                selectedPickable = inventoryBag.GetItemMetaData(idx);
                selected = selectedPickable;
                switch (selectedPickable.worldType)
                {
                    case WorldObject.WorldTypes.WEAPON:
                    case WorldObject.WorldTypes.ARMOR:
                        if (!player.dead)
                        {
                            itemInfo.GetNode<Control>("s/h/v/equip").Show();
                        }
                        break;
                    case WorldObject.WorldTypes.FOOD:
                    case WorldObject.WorldTypes.POTION:
                        if (!inventoryBag.IsSlotCoolingDown(idx) && !player.dead)
                        {
                            itemInfo.GetNode<Label>("s/h/v/use/label").Text =
                                (selectedPickable.worldType == WorldObject.WorldTypes.FOOD) ? "Eat" : "Drink";
                            itemInfo.GetNode<Control>("s/h/v/use").Show();
                        }
                        break;
                }
                if (!player.dead)
                {
                    itemInfo.GetNode<Control>("s/h/v/drop").Show();
                }
            }
            else
            {
                bag = merchantBag;
                merchant.Hide();
                selectedPickable = merchantBag.GetItemMetaData(idx);
                selected = selectedPickable;
                if (selectedPickable is Spell)
                {
                    sndName = "spell_select";
                    bool trained = false;
                    List<Pickable> spells = spellBook.GetItems(false);
                    for (int i = 0; i < spells.Count && !trained; i++)
                    {
                        trained = selectedPickable.Equals(spells[i]);
                    }
                    if (!trained)
                    {
                        itemInfo.GetNode<Label>("s/h/v/buy/label").Text = "Train";
                        ItemInfoHideExcept("buy");
                    }
                }
                else
                {
                    sndName = SndConfigure();
                    ItemInfoHideExcept(
                        (merchant.GetNode<Label>("s/v/label").Text.Equals("Inventory")) ? "sell" : "buy");
                }
            }
            if (!sift)
            {
                Globals.PlaySound(sndName, this, snd);
                itemInfo.GetNode<TextureButton>("s/h/left").Disabled = bag.GetItemSlot(selectedPickable).GetIndex() == 0;
                itemInfo.GetNode<TextureButton>("s/h/right").Disabled = bag.GetItemSlot(selectedPickable).GetIndex() == bag.GetItemCount() - 1;
                selectedPickable.Describe();
                itemInfo.Show();
            }
        }
        public void _OnWeaponSlotPressed()
        {
            ItemInfoGo(player.weapon);
        }
        public void _OnArmorSlotPressed()
        {
            ItemInfoGo(player.vest);
        }
        public void _OnEquipPressed()
        {
            itemInfo.Hide();
            Item selectedPickable = selected as Item;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method _OnEquipPressed");
                return;
            }
            if (!inventoryBag.IsFull())
            {
                switch (selectedPickable.worldType)
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
            else if ((selectedPickable.worldType == WorldObject.WorldTypes.WEAPON & player.weapon != null) ||
                (selectedPickable.worldType == WorldObject.WorldTypes.ARMOR & player.vest != null))
            {
                popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
                return;
            }
            ItemSlot itemSlot = inventoryBag.GetItemSlot(selectedPickable);
            itemSlot.SetBlockSignals(true);
            selectedPickable.Equip();
            itemSlot.SetBlockSignals(false);
            if (!selectedPickable.loaded)
            {
                Globals.PlaySound(SndConfigure(true), this, snd);
            }
            Texture texture = (Texture)GD.Load("res://asset/img/ui/black_bg_icon_used0.tres");
            string path = $"s/v/h/{Enum.GetName(typeof(WorldObject.WorldTypes), selectedPickable.worldType).ToLower()}_slot";
            inventory.GetNode<TextureButton>(path).TextureNormal = texture;
            statsMenu.GetNode<TextureButton>(path).TextureNormal = texture;
            selectedIdx = -1;
            selected = null;
            if (itemInfo.GetNode<Control>("s/h/left").Visible)
            {
                inventory.Show();
            }
            else
            {
                HideMenu();
            }
        }
        public void _OnUnequipPressed()
        {
            if (inventoryBag.IsFull())
            {
                popup.GetNode<Control>("m/error").Show();
            }
            else
            {
                itemInfo.GetNode<Control>("s/h/left").Show();
                itemInfo.GetNode<Control>("s/h/right").Show();
                Globals.PlaySound("click2", this, snd);
                itemInfo.Hide();
                popup.GetNode<Label>("m/yes_no/label").Text = "Unequip?";
                popup.GetNode<Control>("m/yes_no").Show();
            }
            popup.Show();
        }
        public void _OnDropPressed()
        {
            Globals.PlaySound("click2", this, snd);
            itemInfo.Hide();
            popup.GetNode<Label>("m/yes_no/label").Text = "Drop?";
            popup.GetNode<Control>("m/yes_no").Show();
            popup.Show();
        }
        public void _OnUsePressed(bool slotSelect)
        {
            Item selectedItem = selected as Item;
            if (selectedItem == null)
            {
                GD.Print("Unexpected selected type in method _OnUsePressed");
                return;
            }
            switch (selectedItem.worldType)
            {
                case WorldObject.WorldTypes.FOOD:
                    if (player.state == Character.States.ATTACKING)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Cannot Eat\nIn Combat!";
                        popup.GetNode<Control>("m/error").Show();
                        if (!GetTree().Paused)
                        {
                            GetNode<Control>("c/game_menu").Show();
                            selectedIdx = -1;
                            selected = null;
                        }
                        popup.Show();
                        return;
                    }
                    else
                    {
                        Globals.PlaySound("eat", this, snd);
                    }
                    break;
                case WorldObject.WorldTypes.POTION:
                    Globals.PlaySound("drink", this, snd);
                    break;
                default:
                    Globals.PlaySound("click2", this, snd);
                    break;
            }
            Item metaItem = inventoryBag.GetItemMetaData(selectedIdx)as Item;
            if (metaItem != null && selectedItem.worldType == WorldObject.WorldTypes.POTION)
            {
                inventoryBag.SetSlotCoolDown(selectedIdx, metaItem.duration, 0.0f);
            }
            inventoryBag.RemoveItem(selectedIdx, true, false, false);
            selectedItem.Consume(player, 0.0f);
            selectedIdx = -1;
            selected = null;
            if (itemInfo.GetNode("s/h/v/back").IsConnected("pressed", this, nameof(HideMenu)))
            {
                HideMenu();
            }
            else if (!slotSelect)
            {
                itemInfo.Hide();
                inventory.Show();
            }
        }
        public void _OnAcceptPressed()
        {
            Globals.PlaySound("quest_accept", this, snd);
            Globals.worldQuests.StartFocusedQuest();
            QuestEntry questEntry = (QuestEntry)questEntryScene.Instance();
            questEntry.AddToQuestLog(questLog);
            questEntry.quest = Globals.worldQuests.GetFocusedQuest();
            dialogue.GetNode<Control>("s/s/v/accept").Hide();
            dialogue.GetNode<Control>("s/s/v/finish").Hide();
            dialogue.Hide();
            player.target = null;
            HideMenu();
        }
        public void _OnFinishPressed()
        {
            Quest quest = Globals.worldQuests.GetFocusedQuest();
            if (quest.GetGold() > 0)
            {
                Globals.PlaySound("sell_buy", this, snd);
                player.gold = quest.GetGold();
                CombatText combatText = (CombatText)Globals.combatText.Instance();
                player.AddChild(combatText);
                combatText.SetType($"+{quest.GetGold().ToString("N0")}",
                    CombatText.TextType.GOLD, player.GetNode<Node2D>("img").Position);
            }
            Pickable questReward = quest.reward;
            if (questReward != null)
            {
                Globals.map.AddZChild(questReward);
                questReward.GlobalPosition = Globals.map.GetGridPosition(player.GlobalPosition);
            }
            Globals.worldQuests.FinishFocusedQuest();
            if (Globals.worldQuests.GetFocusedQuest() != null)
            {
                dialogue.GetNode<Label>("s/s/label2").Text = Globals.worldQuests.GetFocusedQuest().questStart;
            }
            else
            {
                _OnBackPressed();
            }
        }
        public void _OnBuyPressed()
        {
            Globals.PlaySound("click2", this, snd);
            Pickable selectedPickable = selected as Pickable;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method _OnBuyPressed");
                return;
            }
            itemInfo.Hide();
            if (selectedPickable is Spell && player.level < selectedPickable.level)
            {
                popup.GetNode<Label>("m/error/label").Text = "Can't Learn\nThis Yet!";
                popup.GetNode<Control>("m/error").Show();
            }
            else if (selectedPickable.GetGold() < player.gold)
            {
                popup.GetNode<Label>("m/yes_no/label").Text = (selectedPickable is Item) ? "Buy?" : "Learn?";
                popup.GetNode<Control>("m/yes_no").Show();
            }
            else
            {
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
                popup.GetNode<Control>("m/error").Show();
            }
            popup.Show();
        }
        public void _OnSellPressed()
        {
            Globals.PlaySound("click2", this, snd);
            itemInfo.Hide();
            popup.GetNode<Label>("m/yes_no/label").Text = "Sell?";
            popup.GetNode<Control>("m/yes_no").Show();
            popup.Show();
        }
        public void _OnRepairPressed()
        {
            Globals.PlaySound("click1", this, snd);
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
        public void _OnCastPressed()
        {
            bool showPopup = false;
            Spell selectedSpell = selected as Spell;
            if (selectedSpell == null)
            {
                GD.Print("Unexpected selected type in method _OnCastPressed");
                return;
            }
            if (player.mana >= selectedSpell.manaCost)
            {
                if (player.target != null)
                {
                    if (selectedSpell.requiresTarget && !player.target.enemy)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Invalid\nTarget!";
                        showPopup = true;
                    }
                    else if (player.GetCenterPos().DistanceTo(player.target.GetCenterPos()) > selectedSpell.spellRange &&
                        selectedSpell.spellRange > 0 && selectedSpell.requiresTarget)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Target Not\nIn Range!";
                        showPopup = true;
                    }
                }
                else if (player.target == null && selectedSpell.requiresTarget)
                {
                    popup.GetNode<Label>("m/error/label").Text = "Target\nRequired!";
                    showPopup = true;
                }
            }
            else
            {
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nMana!";
                showPopup = true;
            }
            if (showPopup)
            {
                if (!GetTree().Paused)
                {
                    GetNode<Control>("c/game_menu").Show();
                    selectedIdx = -1;
                    selected = null;
                }
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
                return;
            }
            Globals.PlaySound("click2", this, snd);
            Spell spell = PickableFactory.GetMakeSpell(selectedSpell.worldName);
            spell.GetPickable(player, false);
            spell.ConfigureSpell();
            player.SetCurrentSpell(spell);
            spellBook.SetSlotCoolDown(selectedIdx, spell.cooldown, 0.0f);
            itemInfo.Hide();
            HideMenu();
        }
        public void _OnFilterPressed()
        {
            Globals.PlaySound("click2", this, snd);
            if (questLog.Visible)
            {
                questLog.Hide();
                popup.GetNode<Control>("m/filter_options").Show();
            }
            else
            {
                spellMenu.Hide();
            }
            popup.Show();
        }
        public void _OnGameMenuDraw()
        {
            GetTree().Paused = true;
            listOfMenus.Show();
            if (player.spell != null &&
                player.spell.subType == WorldObject.WorldTypes.CHOOSE_AREA_EFFECT)
            {
                player.spell.UnMake();
            }
            foreach (CanvasItem node in GetNode("c").GetChildren())
            {
                if (node != GetNode("c/game_menu"))
                {
                    node.Hide();
                }
            }
        }
        public void _OnGameMenuHide()
        {
            GetTree().Paused = false;
            GetNode<Control>("c/controls").Show();
            hpMana.Show();
            popup.Hide();
            selectedIdx = -1;
            selected = null;
            foreach (Control node in listOfMenus.GetChildren())
            {
                if (node != menu)
                {
                    node.Hide();
                }
                else
                {
                    node.Show();
                }
            }
        }
        public void _OnDialogueHide()
        {
            dialogue.GetNode<Control>("s/s/v/heal").Hide();
        }
        public void _OnMerchantDraw()
        {
            if (player.target != null)
            {
                if (!(player.weapon != null && player.vest != null) ||
                    player.target.worldType == WorldObject.WorldTypes.TRAINER)
                {
                    merchant.GetNode<Control>("s/v2/repair").Hide();
                    return;
                }
                if (player.weapon != null)
                {
                    if (player.weapon.durability < Item.MAX_DURABILITY)
                    {
                        merchant.GetNode<Control>("s/v2/repair").Show();
                    }
                    else
                    {
                        merchant.GetNode<Control>("s/v2/repair").Hide();
                    }
                }
                if (player.vest != null)
                {
                    if (player.vest.durability < Item.MAX_DURABILITY)
                    {
                        merchant.GetNode<Control>("s/v2/repair").Show();
                    }
                    else if (!merchant.GetNode<Control>("s/v2/repair").Visible ||
                        player.weapon == null)
                    {
                        merchant.GetNode<Control>("s/v2/repair").Hide();
                    }
                }
            }
        }
        public void _OnSiftConfigure(bool right = true)
        {
            Globals.PlaySound("click2", this, snd);
            Pickable selectedPickable = selected as Pickable;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method _OnCastPressed");
                return;
            }
            ItemList bag = (merchantBag.HasItem(selectedPickable)) ? merchantBag : inventoryBag;
            if (spellBook.HasItem(selectedPickable))
            {
                bag = spellBook;
            }
            int index = bag.GetItemSlot(selectedPickable).GetIndex() + 1;
            string leftNodePath = "s/h/left";
            string rightNodePath = "s/h/right";
            if (!right)
            {
                index -= 2;
            }
            if (index >= 0 && index <= bag.GetItemCount() - 1)
            {
                selectedPickable = bag.GetItemMetaData(index);
                selectedIdx = index;
                selectedPickable.Describe();
                if (bag == inventoryBag || bag == merchantBag)
                {
                    _OnBagIndexSelected(index, true);
                }
                else if (bag.IsSlotCoolingDown(index))
                {
                    itemInfo.GetNode<Control>("s/h/v/cast").Hide();
                }
                else if (!player.dead)
                {
                    itemInfo.GetNode<Control>("s/h/v/cast").Show();
                }
            }
            itemInfo.GetNode<TextureButton>(leftNodePath).Disabled = index <= 0;
            itemInfo.GetNode<TextureButton>(rightNodePath).Disabled = index >= bag.GetItemCount() - 1;
        }
        public void _OnUnitHudDraw()
        {
            hpMana.SetSize(new Vector2(720.0f, 185.0f));
        }
        public void _OnUnitHudHide()
        {
            hpMana.SetSize(new Vector2(360.0f, 185.0f));
        }
        public void _OnButtonDown(string nodePath)
        {
            GetNode<Control>(nodePath).RectScale = new Vector2(0.8f, 0.8f);
        }
        public void _OnButtonUp(string nodePath)
        {
            GetNode<Control>(nodePath).RectScale = new Vector2(1.0f, 1.0f);
        }
        public void _OnHudButtonDown(string nodePath)
        {
            ((TextureRect)GetNode(nodePath).GetParent()).Texture = (Texture)GD.Load("res://asset/img/ui/on_screen_button_pressed.tres");
            GetNode<Control>(nodePath).RectScale = new Vector2(0.8f, 0.8f);
        }
        public void _OnHudButtonUp(string nodePath)
        {
            ((TextureRect)GetNode(nodePath).GetParent()).Texture = (Texture)GD.Load("res://asset/img/ui/on_screen_button.tres");
            GetNode<Control>(nodePath).RectScale = new Vector2(1.0f, 1.0f);
        }
        public void _OnMoveHud(bool playerHud = false)
        {
            Vector2 armount;
            Control container;
            if (playerHud)
            {
                container = hpMana.GetNode<Control>("m/h/p/h/g");
                armount = new Vector2(-34.0f, 0.0f);
                if (container.GetRect().Size.x == 100.0f)
                {
                    armount.x = -86.0f;
                }
                if (container.GetRect().Position.x != 0.0f)
                {
                    armount.x = 0.0f;
                }
            }
            else
            {
                container = hpMana.GetNode<Control>("m/h/u/h/g");
                armount = new Vector2(331.0f, 0.0f);
                if (container.GetRect().Size.x == 100.0f)
                {
                    armount.x = 383.0f;
                }
                if (container.GetRect().Position.x != 297.0f)
                {
                    armount.x = 297.0f;
                }
            }
            Tween tween = GetNode<Tween>("tween");
            tween.InterpolateProperty(container, ":rect_position", container.GetRect().Position,
                armount, 0.5f, Tween.TransitionType.Quad, Tween.EaseType.InOut);
            tween.Start();
        }
        public void _OnHealPressed()
        {
            int healerCost = Stats.HealerCost(player.level);
            if (player.gold >= healerCost)
            {
                Globals.PlaySound("sell_buy", this, snd);
                player.gold = -healerCost;
                player.hp = player.hpMax;
                _OnBackPressed();
            }
            else
            {
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
        }
        public void _OnItemInfoLabelDraw()
        {
            Control label = itemInfo.GetNode<Control>("s/v/c/v/c/label");
            label.RectMinSize = ((Control)label.GetParent()).GetRect().Size;
        }
        public void _OnAddToHudPressed()
        {
            Globals.PlaySound("click2", this, snd);
            Pickable selectedPickable = selected as Pickable;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method _OnAddToHudPressed");
                return;
            }
            int count = 1;
            popup.GetNode<Control>("m/add_to_slot/clear_slot").Hide();
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween tween = itemSlot.GetNode<Tween>("tween");
                ColorRect colorRect = itemSlot.GetNode<ColorRect>("m/icon/overlay");
                Label label = itemSlot.GetNode<Label>("m/label");
                if (tween.IsActive())
                {
                    tween.SetActive(false);
                    colorRect.RectScale = new Vector2(1.0f, 1.0f);
                }
                colorRect.Color = new Color(1.0f, 1.0f, 0.0f, 0.75f);
                label.Text = count.ToString();
                label.Show();
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(selectedPickable))
                {
                    popup.GetNode<Control>("m/add_to_slot/clear_slot").Show();
                }
                count++;
            }
            GetNode<Control>("c/controls/right").Hide();
            GetNode<Control>("c/controls").Show();
            popup.GetNode<Control>("m/add_to_slot").Show();
            popup.Show();
        }
        public void _OnDescribePickable(Pickable pickable)
        {
            Pickable selectedPickable = selected as Pickable;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method _OnAddToHudPressed");
                return;
            }
            string menuDescription = pickable.menuDescription;
            if (selectedPickable is Spell && merchantBag.HasItem(pickable))
            {
                RegEx regEx = new RegEx();
                regEx.Compile("-Level: (\\d*)\n");
                string result = regEx.Search(menuDescription).GetString();
                menuDescription = menuDescription.Insert(
                    menuDescription.Find(result) + result.Length,
                    $"-Gold: {pickable.GetGold().ToString("N0")}\n");
            }
            itemInfo.GetNode<Label>("s/v/label").Text = pickable.worldName;
            itemInfo.GetNode<TextureRect>("s/v/c/v/bg/m/icon").Texture = pickable.icon;
            itemInfo.GetNode<RichTextLabel>("s/v/c/v/c/label").Text = menuDescription;
        }
        public void _OnSetPickableInMenu(Pickable pickable, bool stack, Bags bagType)
        {
            switch (bagType)
            {
                case Bags.MERCHANT:
                    merchantBag.AddItem(pickable, stack);
                    break;
                case Bags.INVENTORY:
                    inventoryBag.AddItem(pickable, stack);
                    break;
                case Bags.SPELL:
                    spellBook.AddItem(pickable, stack);
                    break;
            }
        }
        public void _OnEquipItem(Item item, bool on)
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
            string nodePath = $"s/v/h/{Enum.GetName(typeof(WorldObject.WorldTypes), item.worldType).ToLower()}_slot/m/icon";
            inventory.GetNode<TextureRect>(nodePath).Texture = (on) ? item.icon : null;
            statsMenu.GetNode<TextureRect>(nodePath).Texture = (on) ? item.icon : null;
            if (on)
            {
                if (selectedIdx == -1)
                {
                    // this is for if the item is loaded from a save game
                    selectedIdx = inventoryBag.GetItemSlot(selected as Pickable).GetIndex();
                }
                inventoryBag.RemoveItem(selectedIdx);
            }
            else
            {
                inventoryBag.AddItem(item, false);
                ItemSlot itemSlot = inventoryBag.GetItemSlot(item);
                foreach (ItemSlot otherItemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
                {
                    if (otherItemSlot.GetItem() == item &&
                        !itemSlot.IsConnected(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut)))
                    {
                        itemSlot.Connect(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut));
                    }
                }
            }
        }
        public void _OnDropPickable(Pickable pickable)
        {
            inventoryBag.RemoveItem(inventoryBag.GetItemSlot(pickable).GetIndex());
            player.GetNode("inventory").RemoveChild(pickable);
            Globals.map.AddZChild(pickable);
            pickable.Owner = Globals.map;
            pickable.GlobalPosition = Globals.map.SetGetPickableLoc(player.GlobalPosition, true);
        }
        public string SndConfigure(bool byPass = false, bool off = false)
        {
            Pickable selectedPickable = selected as Pickable;
            if (selectedPickable == null)
            {
                GD.Print("Unexpected selected type in method SndConfigure");
                return "";
            }
            string sndName = "";
            if (!byPass && itemInfo.GetNode<Control>("s/h/v/drop").Visible)
            {
                sndName = "inventory_close";
            }
            else
            {
                sndName = $"{ItemDB.GetItemMaterial(selectedPickable.worldName)}_{((off) ? "off" : "on")}";
            }
            return sndName;
        }
        public void UpdateHudIcons(string worldName, Pickable pickable, float seek)
        {
            string iconPath = $"m/h/{((worldName.Equals(player.worldName)) ? 'p' : 'u')}/h/g";
            foreach (ItemSlot itemSlot in hpMana.GetNode(iconPath).GetChildren())
            {
                if (itemSlot.GetItem() == null)
                {
                    // TODO: check these connections, hud slot doesnt dissapear after done
                    // cooling down
                    pickable.Connect(nameof(Pickable.Unmake), itemSlot, nameof(ItemSlot.SetItem),
                        new Godot.Collections.Array() { null, false, true, false });
                    itemSlot.Connect("hide", pickable, nameof(Pickable.UncoupleSlot),
                        new Godot.Collections.Array() { itemSlot });
                    itemSlot.SetItem(pickable);
                    itemSlot.CoolDown(pickable, pickable.duration, seek);
                    itemSlot.Show();
                    break;
                }
            }
        }
        public void UpdateHud(string type, string worldName, int value1, int value2)
        {
            if (type.ToUpper().Equals("ICON_HIDE"))
            {
                string nodePath = (worldName.Equals(player.worldName)) ? "m/h/p/h/g" : "m/h/u/h/g";
                foreach (Node node in hpMana.GetNode(nodePath).GetChildren())
                {
                    ItemSlot itemSlot = node as ItemSlot;
                    if (itemSlot != null)
                    {
                        itemSlot.SetItem(null, false, true, false);
                        itemSlot.Hide();
                    }
                }
            }
            else
            {
                type = type.ToLower();
                string nameLblPath = $"m/h/{(worldName.Equals(player.worldName) ? 'p' : 'u')}/c/bg/m/v/label";
                GD.Print("igm   ", worldName);
                hpMana.GetNode<Label>(nameLblPath).Text = worldName;
                if (!worldName.Equals(player.worldName))
                {
                    merchant.GetNode<Label>("s/v/label").Text = worldName;
                }
                string barNodePath = (worldName.Equals(player.worldName)) ?
                    $"m/h/p/c/bg/m/v/{type}_bar" :
                    $"m/h/u/c/bg/m/v/{type}_bar";
                string lblNodePath = (worldName.Equals(player.worldName)) ?
                    $"m/h/p/c/bg/m/v/{type}_bar/label" :
                    $"m/h/u/c/bg/m/v/{type}_bar/label";
                hpMana.GetNode<TextureProgress>(barNodePath).Value = 100.0f * (float)value1 / (float)value2;
                hpMana.GetNode<Label>(lblNodePath).Text = $"{value1} / {value2}";
            }
        }
        public void ItemInfoGo(Pickable pickable)
        {
            if (pickable != null)
            {
                selected = pickable;
                ItemInfoHideExcept();
                itemInfo.GetNode<Control>("s/h/left").Hide();
                itemInfo.GetNode<Control>("s/h/right").Hide();
                Globals.PlaySound(SndConfigure(false, true), this, snd);
                pickable.Describe();
                if (statsMenu.Visible)
                {
                    statsMenu.Hide();
                    selectedIdx = -2;
                    ItemInfoHideExcept();
                }
                else if (inventory.Visible || selected == null)
                {
                    inventory.Hide();
                    if (!player.dead)
                    {
                        ItemInfoHideExcept("unequip");
                    }
                }
                itemInfo.Show();
            }
        }
        public void HideMenu()
        {
            if (itemInfo.GetNode("s/h/v/back").IsConnected("pressed", this, nameof(HideMenu)))
            {
                Pickable selectedPickable = selected as Pickable;
                if (selectedPickable == null)
                {
                    GD.Print("Unexpected selected type in method HideMenu");
                    return;
                }
                if (itemInfo.Visible)
                {
                    if (spellBook.HasItem(selectedPickable))
                    {
                        Globals.PlaySound("spell_book_close", this, snd);
                    }
                    else
                    {
                        Globals.PlaySound(SndConfigure(true, true), this, snd);
                    }
                }
                Node backButton = itemInfo.GetNode("s/h/v/back");
                backButton.Disconnect("pressed", this, nameof(HideMenu));
                backButton.Connect("pressed", this, nameof(_OnBackPressed));
                itemInfo.Hide();
                itemInfo.GetNode<Control>("s/h/left").Show();
                itemInfo.GetNode<Control>("s/h/right").Show();
            }
            GetNode<Control>("c/game_menu").Hide();
        }
        public void ItemInfoHideExcept(params string[] nodesToShow)
        {
            foreach (Control node in itemInfo.GetNode("s/h/v").GetChildren())
            {
                if (!nodesToShow.Contains(node.Name) && !node.Name.Equals("back"))
                {
                    node.Hide();
                }
                else
                {
                    node.Show();
                }
            }
        }
        public void _OnHudSlotPressed(ItemSlot itemSlot, Pickable pickable)
        {
            selected = pickable;
            if (spellBook.HasItem(pickable))
            {
                selectedIdx = spellBook.GetItemSlot(pickable).GetIndex();
            }
            else if (inventoryBag.HasItem(pickable))
            {
                selectedIdx = inventoryBag.GetItemSlot(pickable).GetIndex();
            }
            if (pickable is Item)
            {
                if (!player.dead && !itemSlot.IsCoolingDown() &&
                    (pickable.worldType == WorldObject.WorldTypes.FOOD ||
                        pickable.worldType == WorldObject.WorldTypes.POTION))
                {
                    _OnUsePressed(true);
                }
                else if (player.weapon == pickable || player.vest == pickable)
                {
                    Node backButton = itemInfo.GetNode("s/h/v/back");
                    backButton.Disconnect("pressed", this, nameof(_OnBackPressed));
                    backButton.Connect("pressed", this, nameof(HideMenu));
                    ItemInfoGo(pickable);
                    ItemInfoHideExcept("unequip");
                    menu.Hide();
                    GetNode<Control>("c/game_menu").Show();
                }
                else
                {
                    PrepItemInfo();
                    Globals.PlaySound(SndConfigure(true, false), this, snd);
                    _OnBagIndexSelected(selectedIdx, true);
                    pickable.Describe();
                    itemInfo.Show();
                }
            }
            else if (itemSlot.IsCoolingDown() || player.dead)
            {
                PrepItemInfo();
                Globals.PlaySound("turn_page", this, snd);
                _OnSpellSelected(selectedIdx, true);
            }
            else
            {
                _OnCastPressed();
            }
            selectedIdx = -1;
        }
        public void ShowQuestText(Quest quest)
        {
            Globals.PlaySound("click2", this, snd);
            selected = questLog;
            dialogue.GetNode<Label>("s/label").Text = quest.questName;
            dialogue.GetNode<RichTextLabel>("s/s/label2").BbcodeText = quest.FormatWithObjectiveText();
            questLog.Hide();
            dialogue.Show();
        }
        private void PrepItemInfo()
        {
            _OnPausePressed(true);
            menu.Hide();
            Node backButton = itemInfo.GetNode("s/h/v/back");
            itemInfo.GetNode<Control>("s/h/left").Hide();
            itemInfo.GetNode<Control>("s/h/right").Hide();
            backButton.Disconnect("pressed", this, nameof(_OnBackPressed));
            backButton.Connect("pressed", this, nameof(HideMenu));
        }
    }
}