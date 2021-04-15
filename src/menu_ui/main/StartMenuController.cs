using Game.Database;
using Godot;
namespace Game.Ui
{
	public class StartMenuController : Control
	{
		private SaveLoadController saveLoadController;
		private AboutController aboutController;
		private PopupController popupController;
		private Control main;

		public override void _Ready()
		{
			GameMenu.player = null;
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
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK0);
			SceneLoaderController.rootNode = GetTree().Root;
			SceneLoaderController.Init().SetScene(string.Format(PathManager.sceneMapPath, "zone_1"), this);
		}
		public void _OnLoadPressed() { Transition(saveLoadController); }
		public void _OnAboutPressed() { Transition(aboutController); }
		public void _OnExitPressed() { GetTree().Quit(); }
		private void Transition(Control scene)
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
			main.Hide();
			scene.Show();
		}
	}
}