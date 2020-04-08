using Godot;
using Game.Actor;
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

        public override void _Ready()
        {
            main = GetNode<Control>("background/margin/main");
            inventory = GetNode<InventoryNode>("background/margin/inventory");
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
            dialogue = GetNode<DialogueNode>("background/margin/dialogue");
            dialogue.Connect("hide", this, nameof(_OnMainMenuHide));
            dialogue.InitMerchantView(
                inventory.GetNode<ItemList>("s/v/c/inventory"),
                spellBook.GetNode<ItemList>("s/v/c/spell_list"));
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
        public void ShowBackground(bool show)
        {
            Color transparent = new Color("00ffffff");
            GetNode<Control>("background").SelfModulate =
                (show) ? new Color("ffffff") : transparent;
            GetNode<ColorRect>("overlay").Color = (show) ? new Color("6e6e6e") : transparent;
        }
        public void _OnNpcInteract(Npc npc)
        {
            if (ContentDB.HasContent(npc.Name))
            {
                main.Hide();
                dialogue.Show();
                dialogue.Display(npc);
                Show();
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
            Globals.SetScene("res://src/menu_ui/start_menu.tscn", GetTree().Root, Globals.map);
        }
        private void Transition(Control scene)
        {
            Globals.PlaySound("click1", this, speaker);
            main.Hide();
            scene.Show();
        }
    }
}