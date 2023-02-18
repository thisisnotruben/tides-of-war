using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Map.Doodads
{
	public class TransitionSign : InteractItem
	{
		private string mapPath = string.Empty;

		public override void _Ready()
		{
			mapPath = string.Format(PathManager.sceneMapPath, Name.Split("-")[1]);
			base._Ready();
			Show();
		}
		protected override void OnSelectPressed() { ShowDialogue(Name); }
		protected override void OnPlayerEntered(Area2D area2D)
		{
			Player player = area2D.Owner as Player;
			if (player != null && !player.dead && !player.attacking)
			{
				ShowInteractAnim(true, true);
			}
		}
		protected override void OnPlayerExited(Area2D area)
		{
			OnDialogueHide();
			ShowInteractAnim(false, true);
		}
		protected override void OnDialogueSignalCallback(object value)
		{
			Name = Name + "-DELETE";
			Globals.sceneLoader.SetScene(mapPath, Map.map, true);
			Player.player.menu.ShowLoadView();
		}
	}
}
