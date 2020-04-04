using Godot;
using Game.Utils;
namespace Game.Ui
{
    public class Popup : Control
    {
        public static MainMenu mainMenu;
        public Speaker speaker= null;

        public void _OnPopupDraw()
        {
            mainMenu.ShowBackground(false);
        }
        public void _OnPopupHide()
        {
            mainMenu.ShowBackground(true);
            foreach (Control control in GetNode("m").GetChildren())
            {
                control.Hide();
            }
        }
        public void _OnRepairDraw()
        {
            int shown = 0;
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
        public void _OnErrorDraw()
        {
            Globals.PlaySound("click6", this, speaker);
        }
        public void _OnMResized()
        {
            GetNode<Control>("bg").RectMinSize = GetNode<Control>("m").RectSize;
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
            Globals.worldQuests.Reset();
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}