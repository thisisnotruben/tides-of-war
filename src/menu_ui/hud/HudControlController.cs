using Godot;
namespace Game.Ui
{
	public class HudControlController : GameMenu
	{
		public Control targetContainer;
		public BaseButton pause;
		public CharacterStatusController playerStatus, targetStatus;

		public override void _Ready()
		{
			// character status
			playerStatus = GetNode<CharacterStatusController>("margin/split/characterStatus/playerStatus");
			targetContainer = GetNode<Control>("margin/split/characterStatus/targetContainer");
			targetStatus = targetContainer.GetChild<CharacterStatusController>(0);

			targetStatus.Connect("visibility_changed", this, nameof(OnTargetStatusVisibilityChanged));
			targetContainer.GetChild<BaseButton>(1).Connect("pressed", this, nameof(ClearTargetStatus));

			pause = GetNode<BaseButton>("margin/split/center/actionBar/pause");
		}
		private void OnTargetStatusVisibilityChanged() { targetContainer.Visible = targetStatus.Visible; }
		public void ClearTargetStatus()
		{
			player.target = null;
			targetContainer.Hide();
			targetStatus.Clear(false);
		}
	}
}