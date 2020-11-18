using Game.Actor;
using Game.Ui;
using Game.Database;
using Godot;
namespace Game.Map.Doodads
{
	public class TransitionZone : Area2D
	{
		public override void _Ready()
		{
			if (!IsConnected("area_entered", this, nameof(_OnPlayerEntered)))
			{
				Connect("area_entered", this, nameof(_OnPlayerEntered));
			}

			if (!IsConnected("area_exited", this, nameof(_OnPlayerExited)))
			{
				Connect("area_exited", this, nameof(_OnPlayerExited));
			}
		}
		public void _OnPlayerEntered(Area2D area)
		{
			Player player = area.Owner as Player;
			if (player == null || player.dead || player.attacking)
			{
				return;
			}

			string sceneMapPath = string.Format(PathManager.sceneMapPath, Name);
			if (ResourceLoader.Exists(sceneMapPath))
			{
				Name = Name + "-DELETE";
				SceneLoaderController.Init().SetScene(sceneMapPath, Map.map, true);
			}
		}
		public void _OnPlayerExited(Area2D area) { }
	}
}
