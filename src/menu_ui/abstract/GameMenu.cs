using Godot;
using Game.Actor;
namespace Game.Ui
{
	public abstract class GameMenu : Control
	{
		public static Player player;

		public virtual void _OnBackPressed()
		{
			Globals.soundPlayer.PlaySound("click3");
			Hide();
		}
	}

}