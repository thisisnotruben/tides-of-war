using Game.Utils;
using Godot;
namespace Game.Ui
{
	public class StartMenu : Control
	{
		private Speaker speaker;
		private SaveLoadNode saveLoad;
		private AboutNode about;
		private Popup popup;
		private Control main;

		public override void _Ready()
		{
			speaker = GetNode<Speaker>("speaker");
			GameMenu.player = null;
			GameMenu.speaker = speaker;
			main = GetNode<Control>("list_of_menus/m/main_menu");
			saveLoad = GetNode<SaveLoadNode>("list_of_menus/m/save_load");
			about = GetNode<AboutNode>("list_of_menus/m/about");
			popup = GetNode<Popup>("popup");
			foreach (Control control in new Control[] { saveLoad, about, popup })
			{
				control.Connect("hide", this, nameof(_OnWindowClosed));
			}
		}
		public void _OnWindowClosed()
		{
			main.Show();
		}
		public void _OnNewGamePressed()
		{
			Globals.PlaySound("click0", this, speaker);
			SceneLoader.rootNode = GetTree().Root;
			SceneLoader.Init().SetScene("res://src/map/zone_3.tscn", this);
		}
		public void _OnLoadPressed()
		{
			Transition(saveLoad);
		}
		public void _OnAboutPressed()
		{
			Transition(about);
		}
		public void _OnExitPressed()
		{
			GetTree().Quit();
		}
		private void Transition(Control scene)
		{
			Globals.PlaySound("click1", this, speaker);
			main.Hide();
			scene.Show();
		}
	}
}