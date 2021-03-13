using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
	public class SceneLoaderController : Node
	{
		private ResourceInteractiveLoader mapLoader;
		private Range progressBar;
		private GC.Dictionary playerState = new GC.Dictionary(),
			loadData = new GC.Dictionary();
		private static Dictionary<string, PackedScene> cache = new Dictionary<string, PackedScene>();
		public static Node rootNode;

		public override void _Ready()
		{
			progressBar = GetNode<Range>("progress_bar/m/v/bar");
			SetProcess(false);
		}
		public static SceneLoaderController Init() { return (SceneLoaderController)SceneDB.sceneLoader.Instance(); }
		private bool TrySetTransitions()
		{
			if (playerState.Count > 0)
			{
				Vector2 spawnLoc = Map.Map.map.GetNode<Node2D>("meta/transitions/" + playerState["_mapName"]).GlobalPosition;
				playerState[NameDB.SaveTag.POSITION] = new GC.Array() { spawnLoc[0], spawnLoc[1] };
				playerState.Remove("_mapName");
				Player.player.Deserialize(playerState);
				playerState.Clear();
				return true;
			}
			return false;
		}
		private bool TrySetLoadData()
		{
			if (loadData.Count > 0)
			{
				foreach (string nodePath in loadData.Keys)
				{
					if (HasNode(nodePath))
					{
						(GetNode(nodePath) as ISerializable)?.Deserialize((GC.Dictionary)loadData[nodePath]);
					}
				}
				loadData.Clear();
				return true;
			}
			return false;
		}
		public override void _Process(float delta)
		{
			switch (mapLoader.Poll())
			{
				case Error.Ok:
					progressBar.Value = 100.0f * mapLoader.GetStage() / mapLoader.GetStageCount();
					break;
				case Error.FileEof:
					PackedScene packedScene = (PackedScene)mapLoader.GetResource();
					cache.Add(mapLoader.GetResource().ResourcePath, packedScene);
					rootNode.AddChild(packedScene.Instance());

					TrySetTransitions();
					TrySetLoadData();
					SetProcess(false);
					QueueFree();
					break;
			}
		}
		public void SetScene(string scenePath, Node currentScene, GC.Dictionary payload)
		{
			loadData = payload;
			SetScene(scenePath, currentScene);
		}
		public void SetScene(string scenePath, Node currentScene, bool transition = false)
		{
			rootNode.AddChild(this);

			if (transition && currentScene is Map.Map)
			{
				playerState = Player.player.Serialize();
				playerState.Add("_mapName", Map.Map.map.Name);
			}

			// load map specific data
			string mapName = scenePath.GetFile().BaseName(),
				unitDataPath = string.Format(PathManager.unitDataTemplate, mapName),
				contentDataPath = string.Format(PathManager.contentDataTemplate, mapName);

			Directory directory = new Directory();
			if (directory.FileExists(unitDataPath))
			{
				Globals.unitDB.LoadData(unitDataPath);
			}
			if (directory.FileExists(contentDataPath))
			{
				Globals.contentDB.LoadData(contentDataPath);
			}

			(currentScene as CanvasItem)?.Hide();
			CallDeferred(nameof(SetSceneDeferred), scenePath, currentScene);
		}
		private void SetSceneDeferred(string scenePath, CanvasItem currentScene)
		{
			currentScene.Free();
			GetTree().Paused = false;
			if (cache.ContainsKey(scenePath))
			{
				GetTree().ChangeSceneTo(cache[scenePath]);
				CallDeferred(nameof(TrySetTransitions));
				CallDeferred(nameof(TrySetLoadData));
				QueueFree();
			}
			else
			{
				mapLoader = ResourceLoader.LoadInteractive(scenePath);
				SetProcess(true);
			}
		}
	}
}