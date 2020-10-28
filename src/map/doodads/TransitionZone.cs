using Game.Actor;
using Game.Ui;
using Godot;
namespace Game.Map.Doodads
{
	public class TransitionZone : Area2D
	{
		public void _OnPlayerEntered(Area2D area)
		{
			Player player = area.Owner as Player;
			if (player == null || player.dead || player.attacking)
			{
				return;
			}

			string sceneMapPath = $"res://src/map/{Name}.tscn";
			if (ResourceLoader.Exists(sceneMapPath))
			{
				Name = Name + "-DELETE";
				SceneLoaderController.Init().SetScene(sceneMapPath, Map.map, true);
			}
		}
		public void _OnPlayerExited(Area2D area) { }
	}
}
