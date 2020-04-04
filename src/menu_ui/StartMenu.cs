using Game.Utils;
using Godot;
namespace Game.Ui
{
    public class StartMenu : Menu
    {
        Speaker speaker;
        public override void _Ready()
        {
            speaker = GetNode<Speaker>("snd");
            listOfMenus = GetNode<Control>("list_of_menus/m");
            saveLoad = listOfMenus.GetNode<SaveLoadNode>("save_load");
            saveLoad.GetNode("v/s/back").Connect("pressed", this, nameof(_OnBackPressed));
            menu = listOfMenus.GetNode<Control>("main_menu");
            about = listOfMenus.GetNode<AboutNode>("about");
            about.speaker = speaker;
            popup = GetNode<Popup>("popup");
            popup.speaker = speaker;
            foreach (Control control in new Control[] {about, popup})
            {
                control.Connect("hide", this, nameof(_OnWindowClosed));
            }
        }
        public void _OnWindowClosed()
        {
            menu.Show();
        }
        public void _OnNewGamePressed()
        {
            Globals.PlaySound("click0", this, speaker);
            Globals.SetScene("res://src/map/zone_4.tscn", GetTree().Root, this);
        }
        public void _OnLoadPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            menu.Hide();
            saveLoad.Show();
        }
        public void _OnSettingsPressed()
        {
            Globals.PlaySound("click1", this, speaker);
        }
        public void _OnAboutPressed()
        {
            Globals.PlaySound("click1", this, speaker);
            menu.Hide();
            about.Show();
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            saveLoad.Hide();
            menu.Show();
        }
        public void _OnExitPressed()
        {
            GetTree().Quit();
        }
    }
}