using Godot;
namespace Game.Ui
{
	public class HudPopupConfirmController : CenterContainer
	{
		public Button button;

		public override void _Ready() { button = GetChild<Button>(0); }
		public void ShowConfirm(string confirmText)
		{
			button.Text = confirmText;
			Show();
		}
	}
}