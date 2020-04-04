using Godot;
using Game.Utils;
using Game.Actor;
namespace Game.Ui
{
    public class MenuHandler : Control
    {
        private Player _player;
        public Player player
        {
            set
            {
                _player = value;
                mainMenu.player = value;
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
                mainMenu.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }
        private MainMenu mainMenu;
        public override void _Ready()
        {
            mainMenu = GetNode<MainMenu>("c/game_menu");
            speaker = GetNode<Speaker>("speaker");
            GetNode<BaseButton>("c/controls/right/pause/icon")
                .Connect("pressed", mainMenu, nameof(_OnShowMainMenu));
        }
        public void _OnShowMainMenu()
        {
            mainMenu.Show();
        }
    }
}