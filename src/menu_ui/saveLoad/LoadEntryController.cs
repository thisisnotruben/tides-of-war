using Godot;
namespace Game.Ui
{
	public class LoadEntryController : Control
	{
		private TextureRect textureRect;
		private Label label;
		public BaseButton[] buttons;
		public int saveIndex = -1;

		public override void _Ready()
		{
			textureRect = GetNode<TextureRect>("colorRect/marginContainer/hBoxContainer/colorRect/marginContainer/textureRect");
			label = GetNode<Label>("colorRect/marginContainer/hBoxContainer/label");
			buttons = new BaseButton[] { textureRect.GetChild<BaseButton>(0), label.GetChild<BaseButton>(1) };
		}
		public void Display(Texture icon, string text)
		{
			textureRect.Texture = icon;
			label.Text = text.Replace(" ", "\n");
		}
	}
}