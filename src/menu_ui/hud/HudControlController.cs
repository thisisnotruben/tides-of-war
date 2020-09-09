using Godot;
namespace Game.Ui
{
	public class HudControlController : Control
	{
		private Texture pressed;
		private Texture normal;

		public override void _Ready()
		{
			pressed = (Texture)GD.Load("res://asset/img/ui/on_screen_button_pressed.tres");
			normal = (Texture)GD.Load("res://asset/img/ui/on_screen_button.tres");
		}
		public void ConnectButtons(Node master, string miniMapMethod, string spellBookMethod, string pauseMethod)
		{
			GetNode<BaseButton>("right/mini_map/icon").Connect("pressed", master, miniMapMethod);
			GetNode<BaseButton>("right/spell_book/icon").Connect("pressed", master, spellBookMethod);
			GetNode<BaseButton>("right/pause/icon").Connect("pressed", master, pauseMethod);
		}
		public void _OnSlotMoved(string nodePath, bool down)
		{
			float scale = (down) ? 0.8f : 1.0f;
			Control icon = GetNode<Control>(nodePath);
			icon.RectScale = new Vector2(scale, scale);
			((TextureRect)icon.GetParent()).Texture = (down) ? pressed : normal;
		}
	}
}