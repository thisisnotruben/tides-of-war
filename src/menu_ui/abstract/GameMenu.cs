using Godot;
using Game.Actor;
using Game.Sound;
namespace Game.Ui
{
	public abstract class GameMenu : Control
	{
		public static Player player;

		public virtual void _OnBackPressed()
		{
			SoundPlayer.INSTANCE.PlaySound("click3");
			Hide();
		}
	}

}