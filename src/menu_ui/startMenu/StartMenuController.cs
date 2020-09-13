using Game.Utils;
using Game.Database;
using Godot;
namespace Game.Ui
{
	public class StartMenuController : Control
	{
		private Speaker speaker;
		private SaveLoadController saveLoadController;
		private AboutController aboutController;
		private PopupController popupController;
		private Control main;

		public override void _Ready()
		{
			GD.PrintT(SpellDB.HasSpell(WorldNameDB.FIREBALL));

			speaker = GetNode<Speaker>("speaker");
			GameMenu.player = null;
			GameMenu.speaker = speaker;
			main = GetNode<Control>("list_of_menus/m/main_menu");
			saveLoadController = GetNode<SaveLoadController>("list_of_menus/m/save_load");
			aboutController = GetNode<AboutController>("list_of_menus/m/about");
			popupController = GetNode<PopupController>("popup");
			foreach (Control control in new Control[] { saveLoadController, aboutController, popupController })
			{
				control.Connect("hide", this, nameof(_OnWindowClosed));
			}
		}
		public void _OnWindowClosed() { main.Show(); }
		public void _OnNewGamePressed()
		{
			Globals.PlaySound("click0", this, speaker);
			SceneLoaderController.rootNode = GetTree().Root;
			SceneLoaderController.Init().SetScene("res://src/map/zone_1.tscn", this);
		}
		public void _OnLoadPressed() { Transition(saveLoadController); }
		public void _OnAboutPressed() { Transition(aboutController); }
		public void _OnExitPressed() { GetTree().Quit(); }
		private void Transition(Control scene)
		{
			Globals.PlaySound("click1", this, speaker);
			main.Hide();
			scene.Show();
		}
	}
}