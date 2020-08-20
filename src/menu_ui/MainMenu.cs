using Godot;
using Game.Actor;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
    public class MainMenu : GameMenu
    {
        private Control main;
        private InventoryNode inventory;
        private StatsNode statsMenu;
        private QuestLogNode questLog;
        private AboutNode about;
        private SaveLoadNode saveLoad;
        private SpellBookNode spellBook;
        private DialogueNode dialogue;
        private Popup popup;
        private ItemList inventoryItemList;
        private ItemList spellItemList;

        public override void _Ready()
        {
            main = GetNode<Control>("background/margin/main");
            inventory = GetNode<InventoryNode>("background/margin/inventory");
            inventoryItemList =  inventory.GetNode<ItemList>("s/v/c/inventory");
            statsMenu = GetNode<StatsNode>("background/margin/stats");
            questLog = GetNode<QuestLogNode>("background/margin/quest_log");
            about = GetNode<AboutNode>("background/margin/about");
            saveLoad = GetNode<SaveLoadNode>("background/margin/save_load");
            popup = GetNode<Popup>("background/margin/popup");
            Popup.mainMenu = this;
            popup.GetNode<BaseButton>("m/exit/exit_game")
                .Connect("pressed", this, nameof(_OnExitGamePressed));
            popup.GetNode<BaseButton>("m/exit/exit_menu")
                .Connect("pressed", this, nameof(_OnExitMenuPressed));
            spellBook = GetNode<SpellBookNode>("background/margin/spell_book");
            spellBook.Connect("hide", this, nameof(_OnMainMenuHide));
            spellItemList = spellBook.itemList;
            dialogue = GetNode<DialogueNode>("background/margin/dialogue");
            dialogue.Connect("hide", this, nameof(_OnMainMenuHide));
            dialogue.InitMerchantView(inventoryItemList, spellItemList);
            foreach (Control node in new Control[] {inventory, statsMenu, questLog, about, saveLoad, popup})
            {
                node.Connect("hide", this, nameof(_OnWindowClosed));
            }
        }
        public void _OnWindowClosed()
        {
            main.Show();
        }
        public void _OnMainMenuDraw()
        {
            GetTree().Paused = true;
            // TODO
            // if (player.spell != null && 
            // player.spell.subType == WorldObject.WorldTypes.CHOOSE_AREA_EFFECT)
            // {
            //     player.spell.UnMake();
            // }
        }
        public void _OnMainMenuHide()
        {
            Hide();
            GetTree().Paused = false;
            main.Show();
            popup.Hide();
        }
        public void ShowSpellBook()
        {
            main.Hide();
            spellBook.Show();
            Show();
        }
        public void ShowBackground(bool show)
        {
            Color transparent = new Color("00ffffff");
            GetNode<Control>("background").SelfModulate =
                (show) ? new Color("ffffff") : transparent;
            GetNode<ColorRect>("overlay").Color = (show) ? new Color("6e6e6e") : transparent;
        }
        public void NpcInteract(Npc npc)
        {
            main.Hide();
            dialogue.Show();
            dialogue.Display(npc);
            Show();
        }
        public void LootInteract(LootChest lootChest)
        {
            if (SpellDB.HasSpell(lootChest.pickableWorldName))
            {
                if (spellItemList.IsFull())
                {
                    popup.GetNode<Label>("m/error/label").Text = "Spell Book\nFull!";
                    popup.GetNode<Control>("m/error").Show();
                    popup.Show();
                }
                else
                {
                    spellItemList.AddItem(lootChest.pickableWorldName, false);
                    lootChest.Collect();
                }
            }
            else
            {
                if (inventoryItemList.IsFull())
                {
                    popup.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
                    popup.GetNode<Control>("m/error").Show();
                    popup.Show();
                }
                else
                {
                    inventoryItemList.AddItem(lootChest.pickableWorldName, true);
                    lootChest.Collect();
                }
            }

        }
        public void _OnResumePressed()
        {
            Globals.PlaySound("click2", this, speaker);
            // TODO
            // player.SetBuff();
            Hide();
        }
        public void _OnInventoryPressed()
        {
            Transition(inventory);
        }
        public void _OnStatsPressed()
        {
            Transition(statsMenu);
        }
        public void _OnQuestLogPressed()
        {
            Transition(questLog);
        }
        public void _OnAboutPressed()
        {
            Transition(about);
        }
        public void _OnSaveLoadPressed()
        {
            Transition(saveLoad);
        } 
        public void _OnExitPressed()
        {
            popup.GetNode<Control>("m/exit").Show();
            Transition(popup);
        }
        public void _OnExitGamePressed()
        {
            GetTree().Quit();
        }
        public void _OnExitMenuPressed()
        {
            Globals.PlaySound("click0", this, speaker);
            GetTree().Paused = false;
            SceneLoader.Init().SetScene("res://src/menu_ui/start_menu.tscn", Map.Map.map);
        }
        private void Transition(Control scene)
        {
            Globals.PlaySound("click1", this, speaker);
            main.Hide();
            scene.Show();
        }
    }
}