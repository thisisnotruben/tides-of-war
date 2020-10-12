using Godot;
namespace Game.Ui
{
	public class HudControlController : Control
	{
		private static Texture normal = (Texture)GD.Load("res://asset/img/ui/on_screen_button_pressed.tres"),
			pressed = (Texture)GD.Load("res://asset/img/ui/on_screen_button.tres");

		private BaseButton pause, spellBook, miniMap;
		public CharacterStatusController playerStatus, targetStatus;

		public override void _Ready()
		{
			// character status
			playerStatus = GetNode<CharacterStatusController>("split/background/margin/split/characterStatus/playerStatus");
			targetStatus = GetNode<CharacterStatusController>("split/background/margin/split/characterStatus/targetStatus");

			// action bar
			pause = GetNode<BaseButton>("split/background/margin/split/center/actionBar/pause/icon");
			spellBook = GetNode<BaseButton>("split/background/margin/split/center/actionBar/spellBook/icon");
			miniMap = GetNode<BaseButton>("split/background/margin/split/center/actionBar/miniMap/icon");

			pause.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { pause.GetPath(), true });
			spellBook.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { spellBook.GetPath(), true });
			miniMap.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { miniMap.GetPath(), true });

			pause.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { pause.GetPath(), false });
			spellBook.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { spellBook.GetPath(), false });
			miniMap.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { miniMap.GetPath(), false });
		}
		public void ConnectButtons(Node master, string miniMapMethod, string spellBookMethod, string pauseMethod)
		{
			pause.Connect("pressed", master, pauseMethod);
			spellBook.Connect("pressed", master, spellBookMethod);
			miniMap.Connect("pressed", master, miniMapMethod);
		}
		public void OnSlotMoved(string nodePath, bool down)
		{
			Control icon = GetNode<Control>(nodePath);
			icon.RectScale = down ? new Vector2(0.8f, 0.8f) : Vector2.One;
			((TextureRect)icon.GetParent()).Texture = down ? pressed : normal;
		}
	}
}