using Godot;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
	public abstract class GameMenu : Control
	{
		public static Player player;

		public virtual void _OnBackPressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK3);
			Hide();
		}
	}

}