using Godot;
namespace Game.Ui
{
	public class LoadEntryController : Control
	{
		private TextureRect textureRect;
		private Label label;
		public Button button;
		public int saveIndex = -1;

		public override void _Ready()
		{
			textureRect = GetNode<TextureRect>("colorRect/marginContainer/hBoxContainer/textureRect");
			label = GetNode<Label>("colorRect/marginContainer/hBoxContainer/label");
			button = GetNode<Button>("colorRect/marginContainer/button");
		}
		public void Display(Texture icon, string text)
		{
			textureRect.Texture = icon;
			label.Text = text;
		}
	}
}