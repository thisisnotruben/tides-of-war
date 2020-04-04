using Game.Utils;
using Godot;
namespace Game.Ui
{
    public class AboutNode : Control
    {
        public Speaker speaker;

        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}