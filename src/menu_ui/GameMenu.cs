using Godot;
using Game.Actor;
using Game.Utils;
namespace Game.Ui
{
    public abstract class GameMenu: Control
    {
        public static Player player;
        public static Speaker speaker;

        public virtual void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
    
}