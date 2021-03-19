using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Game.Mine;
using Game.Factory;
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
			if (loadData.Count == 0)
			{
				return false;
			}

			// load cooldowns first
			string k = NameDB.SaveTag.COOLDOWNS;
			if (loadData.Contains(k))
			{
				Globals.cooldownMaster.Deserialize((GC.Dictionary)loadData[k]);
			}

			GC.Array pos;
			// set all positions first then load rest of data
			foreach (string key in loadData.Keys)
			{
				if (HasNode(key) && GetNode(key) is Character)
				{
					pos = (GC.Array)((GC.Dictionary)loadData[key])[NameDB.SaveTag.POSITION];
					GetNode<Node2D>(key).GlobalPosition = new Vector2((float)pos[0], (float)pos[1]);
				}
			}
			foreach (string key in loadData.Keys)
			{
				if (HasNode(key))
				{
					(GetNode(key) as ISerializable)?.Deserialize((GC.Dictionary)loadData[key]);
				}
			}

			string characterPath, targetPath;
			foreach (string key in loadData.Keys)
			{
				switch (key)
				{
					case NameDB.SaveTag.LAND_MINES:
						foreach (GC.Dictionary landMineData in (GC.Array)loadData[key])
						{
							characterPath = landMineData[NameDB.SaveTag.CHARACTER].ToString();
							if (HasNode(characterPath))
							{
								((LandMine)SceneDB.landMine.Instance()).Init(
									landMineData[NameDB.SaveTag.NAME].ToString(),
									GetNode<Character>(characterPath), false).Deserialize(landMineData);
							}
						}
						break;

					case NameDB.SaveTag.MISSILES:
						MissileFactory missileFactory = new MissileFactory();
						foreach (GC.Dictionary missileData in (GC.Array)loadData[key])
						{
							characterPath = missileData[NameDB.SaveTag.CHARACTER].ToString();
							targetPath = missileData[NameDB.SaveTag.TARGET].ToString();
							if (HasNode(characterPath) && HasNode(targetPath))
							{
								missileFactory.Make(GetNode<Character>(characterPath), GetNode<Character>(targetPath),
									missileData[NameDB.SaveTag.SPELL].ToString()).Deserialize(missileData);
							}
						}
						break;

					case NameDB.SaveTag.CURSOR:
						GC.Dictionary cursorData = (GC.Dictionary)loadData[key];
						if (cursorData.Count > 0)
						{
							pos = (GC.Array)cursorData[NameDB.SaveTag.POSITION];
							((MoveCursorController)SceneDB.moveCursor.Instance()).AddToMap(new Vector2((float)pos[0], (float)pos[1]));
						}
						break;

					case NameDB.SaveTag.TOMB:
						GC.Dictionary tombData = (GC.Dictionary)loadData[key];
						if (tombData.Count > 0)
						{
							pos = (GC.Array)tombData[NameDB.SaveTag.POSITION];
							((Tomb)SceneDB.tomb.Instance()).Init(Player.player, new Vector2((float)pos[0], (float)pos[1]));
						}
						break;
				}
			}
			loadData.Clear();
			return true;
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