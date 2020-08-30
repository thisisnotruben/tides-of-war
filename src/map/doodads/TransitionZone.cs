using Game.Actor;
using Godot;
namespace Game.Map.Doodads
{
	public class TransitionZone : Area2D
	{
		public void _OnPlayerEntered(Area2D area)
		{
			Character character = area.Owner as Character;
			if (character == null || character.dead)
			{
				return;
			}

			string sceneMapPath = $"res://src/map/{Name}.tscn";
			string mapName = Name;
			if (new File().FileExists(sceneMapPath))
			{
				Name = Name + "-DELETE";
				SceneLoader.Init().SetScene(sceneMapPath, Map.map, true);
			}
		}
		public void _OnPlayerExited(Area2D area)
		{

		}
	}
}
