using Godot;
using Game.Actor;
using Game.Utils;
namespace Game.Ui
{
    public class MainMenu : Control
    {
        private Player _player;
        public Player player
        {
            set
            {
                _player = value;
                inventory.player = value;
                statsMenu.player = value;
            }
            get
            {
                return _player;
            }
        }
        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                inventory.speaker = value;
                statsMenu.speaker = value;
                questLog.speaker = value;
                about.speaker = value;
                saveLoad.speaker = value;
                popup.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }
        private Control main;
        private InventoryNode inventory;
        private StatsNode statsMenu;
        private QuestLogNode questLog;
        private AboutNode about;
        private SaveLoadNode saveLoad;
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
            GetTree().Paused = false;
            main.Show();
            popup.Hide();
        }
        public void ShowBackground(bool show)
        {
            GetNode<Control>("background").SelfModulate =
                new Color((show) ? "ffffff" : "00ffffff");
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
        private void Transition(Control scene)
        {
            Globals.PlaySound("click1", this, speaker);
            main.Hide();
            scene.Show();
        }
    }
}