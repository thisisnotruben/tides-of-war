using Godot;

namespace Game.Ui
{
    public class StartMenu : Menu
    {
        public Control settings;

        public override void _Ready()
        {
            listOfMenus = GetNode<Control>("list_of_menus/m");
            saveLoad = listOfMenus.GetNode<SaveLoad>("save_load");
            settings = listOfMenus.GetNode<Control>("settings");
            menu = listOfMenus.GetNode<Control>("main_menu");
            about = listOfMenus.GetNode<Control>("about");
            popup = GetNode<Popup>("popup");
            snd = GetNode<AudioStreamPlayer>("snd");

            saveLoad.GetNode("v/s/back").Connect("pressed", this, nameof(_OnBackPressed));
        }
        public void _OnNewGamePressed()
        {
            Globals.PlaySound("click0", this, snd);
            Globals.SetScene("res://src/map/map-1.tscn", GetTree().GetRoot(), this);
        }
        public void _OnLoadPressed()
        {
            Globals.PlaySound("click1", this, snd);
            if (menu.IsVisible())
            {
                menu.Hide();
                saveLoad.Show();
            }
        }
        public void _OnSettingsPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            settings.Show();
        }
        public void _OnAboutPressed()
        {
            Globals.PlaySound("click1", this, snd);
            menu.Hide();
            about.Show();
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, snd);
            if (saveLoad.IsVisible())
            {
                saveLoad.Hide();
                menu.Show();
            }
            else if (settings.IsVisible())
            {
                settings.Hide();
                menu.Show();
            }
        }
        public void _OnExitPressed()
        {
            GetTree().Quit();
        }
    }
}