using Godot;
namespace Game.Ui
{
	public class HudControlController : GameMenu
	{
		private static Texture normal = GD.Load<Texture>("res://asset/img/ui/on_screen_button_pressed.tres"),
			pressed = GD.Load<Texture>("res://asset/img/ui/on_screen_button.tres");

		private Control targetContainer;
		private BaseButton pause, spellBook, miniMap;
		public CharacterStatusController playerStatus, targetStatus;

		public override void _Ready()
		{
			// character status
			playerStatus = GetNode<CharacterStatusController>("split/background/margin/split/characterStatus/playerStatus");
			targetContainer = GetNode<Control>("split/background/margin/split/characterStatus/targetContainer");
			targetStatus = targetContainer.GetNode<CharacterStatusController>("targetStatus");

			targetStatus.Connect("visibility_changed", this, nameof(_OnTargetStatusVisibilityChanged));
			targetContainer.GetNode<BaseButton>("clearTarget").Connect("pressed", this, nameof(ClearTargetStatus));

			Node actionBar = GetNode("split/background/margin/split/center/actionBar");
			pause = actionBar.GetNode<BaseButton>("pause/icon");
			spellBook = actionBar.GetNode<BaseButton>("spellBook/icon");
			miniMap = actionBar.GetNode<BaseButton>("miniMap/icon");

			pause.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { pause, true });
			spellBook.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { spellBook, true });
			miniMap.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { miniMap, true });

			pause.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { pause, false });
			spellBook.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { spellBook, false });
			miniMap.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { miniMap, false });
		}
		public void ConnectButtons(Node master, string miniMapMethod, string spellBookMethod, string pauseMethod)
		{
			pause.Connect("pressed", master, pauseMethod);
			spellBook.Connect("pressed", master, spellBookMethod);
			miniMap.Connect("pressed", master, miniMapMethod);
		}
		public void _OnTargetStatusVisibilityChanged() { targetContainer.Visible = targetStatus.Visible; }
		public void ClearTargetStatus()
		{
			player.target = null;
			targetContainer.Hide();
			targetStatus.Clear(false);
		}
	}
}