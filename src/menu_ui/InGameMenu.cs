using Godot;
using Game.Actor;
using Game.Misc.Loot;
using Game.Misc.Other;
using Game.Quests;
using Game.Database;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Game.Ui
{
    public class InGameMenu : Menu
    {
        private static readonly PackedScene questEntryScene = (PackedScene)GD.Load("res://src/menu_ui/quest_entry.tscn");
        public Control spellMenu;
        public Control hpMana;
        public enum Bags : byte { INVENTORY, MERCHANT, SPELL };

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
            snd = GetNode<AudioStreamPlayer>("snd");

            saveLoad.GetNode("v/s/back").Connect("pressed", this, nameof(_OnBackPressed));

            player = (Player)GetOwner();
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
                                itemSlot.Connect(nameof(ItemSlot.Cooldown), hudSlot, nameof(ItemSlot.CoolDown));
                                hudSlot.Connect(nameof(ItemSlot.Cooldown), itemSlot, nameof(ItemSlot.CoolDown));
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
                        scrollBar.SetModulate(new Color(1.0f, 1.0f, 1.0f, 0.0f));
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
            if (merchant.IsVisible())
            {
                merchantBag.Clear();
                merchant.GetNode<Label>("s/v/label").SetText("Inventory");
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
            merchant.GetNode<Label>("s/v/label").SetText(player.GetTarget().GetWorldName());
            merchant.GetNode<Control>("s/v2/merchant").Hide();
            merchant.GetNode<Control>("s/v2/inventory").Show();
            foreach (Node node in player.GetTarget().GetNode("inventory").GetChildren())
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
            string statsDescription = $"Name: {player.GetWorldName()}\nHealth: {player.hp} / {player.hpMax}\n" +
                $"Mana: {player.mana} / {player.manaMax}\nXP: {player.xp}\nLevel: {player.GetLevel()}\n" +
                $"Gold: {player.GetGold().ToString("N0")}\nStamina: {player.stamina}\nIntellect: {player.intellect}\n" +
                $"Agility: {player.agility}\nArmor: {player.armor}\nDamage: {player.minDamage} - {player.maxDamage}\n" +
                $"Attack Speed: {player.weaponSpeed.ToString("0.00")}\nAttack Range: {player.weaponRange}";
            statsMenu.GetNode<RichTextLabel>("s/v/c/label").SetBbcode(statsDescription);
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
            spellMenu.GetNode<Label>("s/v/m/v/label").SetText($"Health: {player.hp} / {player.hpMax}");
            spellMenu.GetNode<Label>("s/v/m/v/label2").SetText($"Mana: {player.mana} / {player.manaMax}");
            menu.Hide();
            GetNode<Control>("c/game_menu").Show();
            spellMenu.Show();
        }
        public void _OnMiniMapPressed()
        {
            Sprite miniMap = GetNode<Sprite>("c/mini_map/map");
            if (miniMap.GetTexture() != null)
            {
                Globals.PlaySound("click5", this, snd);
                if (player.GetSpell() != null
                && player.GetSpell().GetPickableSubType() == WorldObject.WorldTypes.CHOOSE_AREA_EFFECT)
                {
                    player.GetSpell().UnMake();
                }
                else if (miniMap.IsVisible())
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
            if (player.GetTarget() != null)
            {
                Globals.PlaySound("click5", this, snd);
                player.SetTarget(null);
            }
        }
        public void _OnBackPressed()
        {
            string sndName = "click3";
            if (inventory.IsVisible())
            {
                inventory.Hide();
                menu.Show();
            }
            else if (statsMenu.IsVisible())
            {
                statsMenu.Hide();
                menu.Show();
            }
            else if (questLog.IsVisible())
            {
                foreach (Control control in questLog.GetNode("s/v/s/v").GetChildren())
                {
                    control.Show();
                }
                questLog.Hide();
                menu.Show();
            }
            else if (saveLoad.IsVisible())
            {
                saveLoad.Hide();
                menu.Show();
            }
            else if (spellMenu.IsVisible())
            {
                sndName = "spell_book_close";
                spellMenu.Hide();
                HideMenu();
            }
            else if (itemInfo.IsVisible())
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
                        if (!player.GetNode("inventory").GetChildren().Contains(selectedPickable)
                        || merchant.GetNode<Label>("s/v/label").GetText().Equals("Inventory"))
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
                    GD.Print("Unexpected type in method _OnBackPressed on condition itemInfo.IsVisible()");
                }
                selected = null;
                selectedIdx = -1;
            }
            else if (dialogue.IsVisible())
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
                    player.SetTarget(null);
                    HideMenu();
                }
            }
            else if (merchant.IsVisible())
            {
                if (player.GetTarget() != null)
                {
                    if (player.GetTarget().GetWorldType() == WorldObject.WorldTypes.TRAINER)
                    {
                        itemInfo.GetNode<Label>("s/h/v/buy/label").SetText("Buy");
                        popup.GetNode<Label>("m/yes_no/label").SetText("Buy?");
                        sndName = "spell_book_close";
                    }
                    else
                    {
                        sndName = "merchant_close";
                    }
                }
                itemInfo.GetNode<TextureButton>("s/v/c/v/bg").SetDisabled(false);
                merchant.GetNode<Label>("s/v/label").SetText("");
                merchant.GetNode<Control>("s/v2/merchant").Hide();
                merchant.GetNode<Control>("s/v2/repair").Hide();
                merchant.GetNode<Control>("s/v2/inventory").Show();
                merchant.Hide();
                merchantBag.Clear();
                player.SetTarget(null);
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
            if (spellBook.IsSlotCoolingDown(idx) || player.IsDead())
            {
                ItemInfoHideExcept();
            }
            else
            {
                ItemInfoHideExcept("cast");

            }
            selectedPickable.Describe();
            if (selectedPickable.GetIndex() == 0)
            {
                itemInfo.GetNode<TextureButton>("s/h/left").SetDisabled(true);
            }
            if (spellBook.GetItemCount() - 1 == idx)
            {
                itemInfo.GetNode<TextureButton>("s/h/right").SetDisabled(true);
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
                switch (selectedPickable.GetWorldType())
                {
                    case WorldObject.WorldTypes.WEAPON:
                    case WorldObject.WorldTypes.ARMOR:
                        if (!player.IsDead())
                        {
                            itemInfo.GetNode<Control>("s/h/v/equip").Show();
                        }
                        break;
                    case WorldObject.WorldTypes.FOOD:
                    case WorldObject.WorldTypes.POTION:
                        if (!inventoryBag.IsSlotCoolingDown(idx) && !player.IsDead())
                        {
                            itemInfo.GetNode<Label>("s/h/v/use/label").SetText(
                                (selectedPickable.GetWorldType() == WorldObject.WorldTypes.FOOD) ? "Eat" : "Drink");
                            itemInfo.GetNode<Control>("s/h/v/use").Show();
                        }
                        break;
                }
                if (!player.IsDead())
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
                if (selectedPickable is Spell.Spell)
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
                        itemInfo.GetNode<Label>("s/h/v/buy/label").SetText("Train");
                        ItemInfoHideExcept("buy");
                    }
                }
                else
                {
                    sndName = SndConfigure();
                    ItemInfoHideExcept(
                        (merchant.GetNode<Label>("s/v/label").GetText().Equals("Inventory")) ? "sell" : "buy");
                }
            }
            if (!sift)
            {
                Globals.PlaySound(sndName, this, snd);
                itemInfo.GetNode<TextureButton>("s/h/left").SetDisabled(
                    bag.GetItemSlot(selectedPickable).GetIndex() == 0);
                itemInfo.GetNode<TextureButton>("s/h/right").SetDisabled(
                    bag.GetItemSlot(selectedPickable).GetIndex() == bag.GetItemCount() - 1);
                selectedPickable.Describe();
                itemInfo.Show();
            }
        }
        public void _OnWeaponSlotPressed()
        {
            ItemInfoGo(player.GetWeapon());
        }
        public void _OnArmorSlotPressed()
        {
            ItemInfoGo(player.GetArmor());
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
                switch (selectedPickable.GetWorldType())
                {
                    case WorldObject.WorldTypes.WEAPON:
                        if (player.GetWeapon() != null)
                        {
                            player.GetWeapon().Unequip();
                        }
                        break;
                    case WorldObject.WorldTypes.ARMOR:
                        if (player.GetArmor() != null)
                        {
                            player.GetArmor().Unequip();
                        }
                        break;
                }
            }
            else if ((selectedPickable.GetWorldType() == WorldObject.WorldTypes.WEAPON & player.GetWeapon() != null)
            || (selectedPickable.GetWorldType() == WorldObject.WorldTypes.ARMOR & player.GetArmor() != null))
            {
                popup.GetNode<Label>("m/error/label").SetText("Inventory\nFull!");
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
            Texture texture = (Texture)GD.Load("res://asset/img/ui/black_bg_icon_used0.res");
            string path = $"s/v/h/{Enum.GetName(typeof(WorldObject.WorldTypes), selectedPickable.GetWorldType()).ToLower()}_slot";
            inventory.GetNode<TextureButton>(path).SetNormalTexture(texture);
            statsMenu.GetNode<TextureButton>(path).SetNormalTexture(texture);
            selectedIdx = -1;
            selected = null;
            if (itemInfo.GetNode<Control>("s/h/left").IsVisible())
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
                popup.GetNode<Label>("m/yes_no/label").SetText("Unequip?");
                popup.GetNode<Control>("m/yes_no").Show();
            }
            popup.Show();
        }
        public void _OnDropPressed()
        {
            Globals.PlaySound("click2", this, snd);
            itemInfo.Hide();
            popup.GetNode<Label>("m/yes_no/label").SetText("Drop?");
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
            switch (selectedItem.GetWorldType())
            {
                case WorldObject.WorldTypes.FOOD:
                    if (player.GetState() == Character.States.ATTACKING)
                    {
                        popup.GetNode<Label>("m/error/label").SetText("Cannot Eat\nIn Combat!");
                        popup.GetNode<Control>("m/error").Show();
                        if (!GetTree().IsPaused())
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
            Item metaItem = inventoryBag.GetItemMetaData(selectedIdx) as Item;
            if (metaItem != null && selectedItem.GetWorldType() == WorldObject.WorldTypes.POTION)
            {
                inventoryBag.SetSlotCoolDown(selectedIdx, metaItem.GetDuration(), 0.0f);
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
            Globals.GetWorldQuests().StartFocusedQuest();
            QuestEntry questEntry = (QuestEntry)questEntryScene.Instance();
            questEntry.AddToQuestLog(questLog);
            questEntry.SetQuest(Globals.GetWorldQuests().GetFocusedQuest());
            dialogue.GetNode<Control>("s/s/v/accept").Hide();
            dialogue.GetNode<Control>("s/s/v/finish").Hide();
            dialogue.Hide();
            player.SetTarget(null);
            HideMenu();
        }
        public void _OnFinishPressed()
        {
            Quest quest = Globals.GetWorldQuests().GetFocusedQuest();
            if (quest.GetGold() > 0)
            {
                Globals.PlaySound("sell_buy", this, snd);
                player.SetGold(quest.GetGold());
                CombatText combatText = (CombatText)Globals.combatText.Instance();
                player.AddChild(combatText);
                combatText.SetType($"+{quest.GetGold().ToString("N0")}",
                    CombatText.TextType.GOLD, player.GetNode<Node2D>("img").GetPosition());
            }
            Pickable questReward = quest.GetReward();
            if (questReward != null)
            {
                Globals.GetMap().GetNode("zed/z1").AddChild(questReward);
                questReward.SetGlobalPosition(
                    Globals.GetMap().GetGridPosition(player.GetGlobalPosition()));
            }
            Globals.GetWorldQuests().FinishFocusedQuest();
            if (Globals.GetWorldQuests().GetFocusedQuest() != null)
            {
                dialogue.GetNode<Label>("s/s/label2").SetText(
                        Globals.GetWorldQuests().GetFocusedQuest().GetQuestStartText());
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
            if (selectedPickable is Spell.Spell && player.GetLevel() < selectedPickable.GetLevel())
            {
                popup.GetNode<Label>("m/error/label").SetText("Can't Learn\nThis Yet!");
                popup.GetNode<Control>("m/error").Show();
            }
            else if (selectedPickable.GetGold() < player.GetGold())
            {
                if (selectedPickable is Item)
                {
                    popup.GetNode<Label>("m/yes_no/label").SetText("Buy?");
                }
                else
                {
                    popup.GetNode<Label>("m/yes_no/label").SetText("Learn?");
                }
                popup.GetNode<Control>("m/yes_no").Show();
            }
            else
            {
                popup.GetNode<Label>("m/error/label").SetText("Not Enough\nGold!");
                popup.GetNode<Control>("m/error").Show();
            }
            popup.Show();
        }
        public void _OnSellPressed()
        {
            Globals.PlaySound("click2", this, snd);
            itemInfo.Hide();
            popup.GetNode<Label>("m/yes_no/label").SetText("Sell?");
            popup.GetNode<Control>("m/yes_no").Show();
            popup.Show();
        }
        public void _OnRepairPressed()
        {
            Globals.PlaySound("click1", this, snd);
            popup.GetNode<Control>("m/repair").Show();
            string text = "";
            if (player.GetWeapon() == null)
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_weapon").Show();
                text = $"Weapon: {Stats.ItemRepairCost(player.GetWeapon().GetLevel())}";
            }
            if (player.GetArmor() == null)
            {
                popup.GetNode<Control>("m/repair/repair_armor").Hide();
                popup.GetNode<Control>("m/repair/repair_all").Hide();
            }
            else
            {
                popup.GetNode<Control>("m/repair/repair_armor").Show();
                short armorCost = Stats.ItemRepairCost(player.GetArmor().GetLevel());
                text += (player.GetWeapon() == null) ? $"Armor: {armorCost}" : $"\nArmor: {armorCost}";
            }
            if (player.GetWeapon() != null && player.GetArmor() != null)
            {
                int total = Stats.ItemRepairCost(player.GetArmor().GetLevel()) + Stats.ItemRepairCost(player.GetWeapon().GetLevel());
                text += $"\nAll: {total}";
            }
            popup.GetNode<Label>("m/repair/label").SetText(text);
            popup.Show();
        }
        public void _OnCastPressed()
        {
            bool showPopup = false;
            Spell.Spell selectedSpell = selected as Spell.Spell;
            if (selectedSpell == null)
            {
                GD.Print("Unexpected selected type in method _OnCastPressed");
                return;
            }
            if (player.mana >= selectedSpell.GetManaCost())
            {
                if (player.GetTarget() != null)
                {
                    if (selectedSpell.RequiresTarget() && !player.GetTarget().IsEnemy())
                    {
                        popup.GetNode<Label>("m/error/label").SetText("Invalid\nTarget!");
                        showPopup = true;
                    }

                    else if (player.GetCenterPos().DistanceTo(player.GetTarget().GetCenterPos()) > selectedSpell.GetSpellRange()
                    && selectedSpell.GetSpellRange() > 0 && selectedSpell.RequiresTarget())
                    {
                        popup.GetNode<Label>("m/error/label").SetText("Target Not\nIn Range!");
                        showPopup = true;
                    }
                }
                else if (player.GetTarget() == null && selectedSpell.RequiresTarget())
                {
                    popup.GetNode<Label>("m/error/label").SetText("Target\nRequired!");
                    showPopup = true;
                }
            }
            else
            {
                popup.GetNode<Label>("m/error/label").SetText("Not Enough\nMana!");
                showPopup = true;
            }
            if (showPopup)
            {
                if (!GetTree().IsPaused())
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
            Spell.Spell spell = PickableFactory.GetMakeSpell(selectedSpell.GetWorldName());
            spell.GetPickable(player, false);
            spell.ConfigureSpell();
            player.SetSpell(spell);
            spellBook.SetSlotCoolDown(selectedIdx, spell.GetCoolDownTime(), 0.0f);
            itemInfo.Hide();
            HideMenu();
        }
        public void _OnFilterPressed()
        {
            Globals.PlaySound("click2", this, snd);
            if (questLog.IsVisible())
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
            GetTree().SetPause(true);
            listOfMenus.Show();
            if (player.GetSpell() != null
            && player.GetSpell().GetPickableSubType() == WorldObject.WorldTypes.CHOOSE_AREA_EFFECT)
            {
                player.GetSpell().UnMake();
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
            GetTree().SetPause(false);
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
            if (player.GetTarget() != null)
            {
                if (!(player.GetWeapon() != null && player.GetArmor() != null)
                || player.GetTarget().GetWorldType() == WorldObject.WorldTypes.TRAINER)
                {
                    merchant.GetNode<Control>("s/v2/repair").Hide();
                    return;
                }
                if (player.GetWeapon() != null)
                {
                    if (player.GetWeapon().GetDurability() < Item.MAX_DURABILITY)
                    {
                        merchant.GetNode<Control>("s/v2/repair").Show();
                    }
                    else
                    {
                        merchant.GetNode<Control>("s/v2/repair").Hide();
                    }
                }
                if (player.GetArmor() != null)
                {
                    if (player.GetArmor().GetDurability() < Item.MAX_DURABILITY)
                    {
                        merchant.GetNode<Control>("s/v2/repair").Show();
                    }
                    else if (!merchant.GetNode<Control>("s/v2/repair").IsVisible()
                    || player.GetWeapon() == null)
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
                else if (!player.IsDead())
                {
                    itemInfo.GetNode<Control>("s/h/v/cast").Show();
                }
            }
            itemInfo.GetNode<TextureButton>(leftNodePath).SetDisabled(index <= 0);
            itemInfo.GetNode<TextureButton>(rightNodePath).SetDisabled(index >= bag.GetItemCount() - 1);
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
            GetNode<Control>(nodePath).SetScale(new Vector2(0.8f, 0.8f));
        }
        public void _OnButtonUp(string nodePath)
        {
            GetNode<Control>(nodePath).SetScale(new Vector2(1.0f, 1.0f));
        }
        public void _OnHudButtonDown(string nodePath)
        {
            ((TextureRect)GetNode(nodePath).GetParent())
                .SetTexture((Texture)GD.Load("res://asset/img/ui/on_screen_button_pressed.res"));
            GetNode<Control>(nodePath).SetScale(new Vector2(0.8f, 0.8f));
        }
        public void _OnHudButtonUp(string nodePath)
        {
            ((TextureRect)GetNode(nodePath).GetParent())
                .SetTexture((Texture)GD.Load("res://asset/img/ui/on_screen_button.res"));
            GetNode<Control>(nodePath).SetScale(new Vector2(1.0f, 1.0f));
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
            short healerCost = Stats.HealerCost(player.GetLevel());
            if (player.GetGold() >= healerCost)
            {
                Globals.PlaySound("sell_buy", this, snd);
                player.SetGold((short)-healerCost);
                player.SetHp(player.hpMax);
                _OnBackPressed();
            }
            else
            {
                popup.GetNode<Label>("m/error/label").SetText("Not Enough\nGold!");
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
        }
        public void _OnItemInfoLabelDraw()
        {
            Control label = itemInfo.GetNode<Control>("s/v/c/v/c/label");
            label.SetCustomMinimumSize(((Control)label.GetParent()).GetRect().Size);
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
            short count = 1;
            popup.GetNode<Control>("m/add_to_slot/clear_slot").Hide();
            foreach (ItemSlot itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
            {
                Tween tween = itemSlot.GetNode<Tween>("tween");
                ColorRect colorRect = itemSlot.GetNode<ColorRect>("m/icon/overlay");
                Label label = itemSlot.GetNode<Label>("m/label");
                if (tween.IsActive())
                {
                    tween.SetActive(false);
                    colorRect.SetScale(new Vector2(1.0f, 1.0f));
                }
                colorRect.SetFrameColor(new Color(1.0f, 1.0f, 0.0f, 0.75f));
                label.SetText(count.ToString());
                label.Show();
                if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(selectedPickable))
                {
                    popup.GetNode<Control>("m/add_to_slot/clear_slot");
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
            string pickableDescription = pickable.GetPickableWorldDescription();
            if (selectedPickable is Spell.Spell && merchantBag.HasItem(pickable))
            {
                RegEx regEx = new RegEx();
                regEx.Compile("-Level: (\\d*)\n");
                string result = regEx.Search(pickableDescription).GetString();
                pickableDescription = pickableDescription.Insert(
                    pickableDescription.Find(result) + result.Length,
                        $"-Gold: {pickable.GetGold().ToString("N0")}\n");
            }
            itemInfo.GetNode<Label>("s/v/label").SetText(pickable.GetWorldName());
            itemInfo.GetNode<TextureRect>("s/v/c/v/bg/m/icon").SetTexture(pickable.GetIcon());
            itemInfo.GetNode<RichTextLabel>("s/v/c/v/c/label").SetText(pickableDescription);
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
            switch (item.GetWorldType())
            {
                case WorldObject.WorldTypes.WEAPON:
                    Tuple<short, short> values = item.GetValues();
                    player.SetWeapon((on) ? item : null);
                    player.minDamage += (on) ? values.Item1 : (short)-values.Item1;
                    player.maxDamage += (on) ? values.Item2 : (short)-values.Item2;
                    break;
                case WorldObject.WorldTypes.ARMOR:
                    player.SetArmor((on) ? item : null);
                    player.armor += (on) ? item.GetValue() : (short)-item.GetValue();
                    break;
            }
            string nodePath = $"s/v/h/{Enum.GetName(typeof(WorldObject.WorldTypes), item.GetWorldType()).ToLower()}_slot/m/icon";
            inventory.GetNode<TextureRect>(nodePath).SetTexture((on) ? item.GetIcon() : null);
            statsMenu.GetNode<TextureRect>(nodePath).SetTexture((on) ? item.GetIcon() : null);
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
                    if (otherItemSlot.GetItem() == item
                    && !itemSlot.IsConnected(nameof(ItemSlot.SyncSlot), otherItemSlot, nameof(ItemSlot._OnSyncShortcut)))
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
            Globals.GetMap().GetNode("zed/z1").AddChild(pickable);
            pickable.SetOwner(Globals.GetMap());
            GD.Print("hrtrtr");
            pickable.SetGlobalPosition(Globals.GetMap().SetGetPickableLoc(player.GetGlobalPosition(), true));
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
            if (!byPass && itemInfo.GetNode<Control>("s/h/v/drop").IsVisible())
            {
                sndName = "inventory_close";
            }
            else
            {
                switch (selectedPickable.GetWorldType())
                {
                    case WorldObject.WorldTypes.ARMOR:
                        sndName = $"{ItemDB.GetItemArmorMaterial(selectedPickable.GetWorldName())}_on";
                        break;
                    case WorldObject.WorldTypes.WEAPON:
                        switch (selectedPickable.GetPickableSubType())
                        {
                            case WorldObject.WorldTypes.STAFF:
                            case WorldObject.WorldTypes.BOW:
                                sndName = "wood_on";
                                break;
                            default:
                                sndName = "metal1_on";
                                break;
                        }
                        break;
                    case WorldObject.WorldTypes.POTION:
                        sndName = "glass_on";
                        break;
                    default:
                        sndName = "misc_on";
                        break;
                }
            }
            if (off)
            {
                sndName = sndName.Replace("on", "off");
            }
            return sndName;
        }
        public void UpdateHudIcons(string worldName, Pickable pickable, float seek)
        {
            string iconPath = $"m/h/{((worldName.Equals(player.GetWorldName())) ? 'p' : 'u')}/h/g";
            foreach (ItemSlot itemSlot in hpMana.GetNode(iconPath).GetChildren())
            {
                if (itemSlot.GetItem() == null)
                {
                    pickable.Connect(nameof(Pickable.Unmake), itemSlot, nameof(ItemSlot.SetItem),
                        new Godot.Collections.Array() { null, false, true });
                    itemSlot.Connect("hide", pickable, nameof(Pickable.UncoupleSlot),
                        new Godot.Collections.Array() { itemSlot });
                    itemSlot.SetItem(pickable);
                    itemSlot.CoolDown(pickable, pickable.GetDuration(), seek);
                    itemSlot.Show();
                    break;
                }
            }
        }
        public void UpdateHud(string type, string worldName, short value1, short value2)
        {
            if (type.ToUpper().Equals("ICON_HIDE"))
            {
                string nodePath = (worldName.Equals(player.GetWorldName())) ? "m/h/p/h/g" : "m/h/u/h/g";
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
                string nameLblPath = $"m/h/{(worldName.Equals(player.GetWorldName()) ? 'p' : 'u')}/c/bg/m/v/label";
                hpMana.GetNode<Label>(nameLblPath).SetText(worldName);
                if (!worldName.Equals(player.GetWorldName()))
                {
                    merchant.GetNode<Label>("s/v/label").SetText(worldName);
                }
                string barNodePath = (worldName.Equals(player.GetWorldName()))
                ? $"m/h/p/c/bg/m/v/{type}_bar"
                : $"m/h/u/c/bg/m/v/{type}_bar";
                string lblNodePath = (worldName.Equals(player.GetWorldName()))
                ? $"m/h/p/c/bg/m/v/{type}_bar/label"
                : $"m/h/u/c/bg/m/v/{type}_bar/label";
                hpMana.GetNode<TextureProgress>(barNodePath).SetValue(
                        100.0f * (float)value1 / (float)value2);
                hpMana.GetNode<Label>(lblNodePath).SetText($"{value1} / {value2}");
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
                if (statsMenu.IsVisible())
                {
                    statsMenu.Hide();
                    selectedIdx = -2;
                    ItemInfoHideExcept();
                }
                else if (inventory.IsVisible() || selected == null)
                {
                    inventory.Hide();
                    if (!player.IsDead())
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
                if (itemInfo.IsVisible())
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
                if (!nodesToShow.Contains(node.GetName()) && !node.GetName().Equals("back"))
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
                if (!player.IsDead() && !itemSlot.IsCoolingDown()
                && (pickable.GetWorldType() == WorldObject.WorldTypes.FOOD
                || pickable.GetWorldType() == WorldObject.WorldTypes.POTION))
                {
                    _OnUsePressed(true);
                }
                else if (player.GetWeapon() == pickable || player.GetArmor() == pickable)
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
            else if (itemSlot.IsCoolingDown() || player.IsDead())
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
            dialogue.GetNode<Label>("s/label").SetText(quest.GetQuestName());
            dialogue.GetNode<RichTextLabel>("s/s/label2").SetBbcode(quest.FormatWithObjectiveText());
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