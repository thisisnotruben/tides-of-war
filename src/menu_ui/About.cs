using Godot;

namespace Game.Ui
{
    public class About : Control
    {
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, new AudioStreamPlayer());
            Hide();
            ((Menu)GetOwner()).menu.Show();
        }
    }
}