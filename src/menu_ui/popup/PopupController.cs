using Godot;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		public static MainMenuController mainMenuController;

		public void _OnPopupDraw() { mainMenuController?.ShowBackground(false); }
		public void _OnPopupHide()
		{
			mainMenuController?.ShowBackground(true);
			foreach (Control control in GetNode("m").GetChildren())
			{
				control.Hide();
			}
		}
		public void _OnErrorDraw() { Globals.soundPlayer.PlaySound("click6"); }
		public void _OnMResized() { GetNode<Control>("bg").RectMinSize = GetNode<Control>("m").RectSize; }
		public void _OnRepairDraw()
		{
			int shown = 0;
			foreach (Control node in GetNode("m/repair").GetChildren())
			{
				if (node.Visible)
				{
					shown++;
				}
			}
			if (shown > 4)
			{
				GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey2_bg.tres");
			}
		}
		public void _OnRepairHide() { GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey3_bg.tres"); }
	}
}