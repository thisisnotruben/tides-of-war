using Godot;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		public static MainMenu mainMenu;

		public void _OnPopupDraw()
		{
			if (mainMenu != null)
			{
				mainMenu.ShowBackground(false);
			}
		}
		public void _OnPopupHide()
		{
			if (mainMenu != null)
			{
				mainMenu.ShowBackground(true);
			}
			foreach (Control control in GetNode("m").GetChildren())
			{
				control.Hide();
			}
		}
		public void _OnErrorDraw() { Globals.PlaySound("click6", this, speaker); }
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