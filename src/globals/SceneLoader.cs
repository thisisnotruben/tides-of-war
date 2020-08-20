using System.Collections.Generic;
using Godot;
using Game.Actor;
using Game.Database;
namespace Game
{
    public class SceneLoader : Node
    {
        private ResourceInteractiveLoader mapLoader;
        private Range progressBar;
        private Godot.Collections.Dictionary playerState = new Godot.Collections.Dictionary();
        private static Dictionary<string, PackedScene> cache = new Dictionary<string, PackedScene>();
        public static Node rootNode;

        public override void _Ready()
        {
            progressBar = GetNode<Range>("progress_bar/m/v/bar");
            SetProcess(false);
        }
        public static SceneLoader Init()
        {
            PackedScene sceneLoaderScene = (PackedScene)GD.Load("res://src/globals/scene_loader.tscn");
            return (SceneLoader) sceneLoaderScene.Instance();
        }
        private void SetTransitions()
        {
            if (playerState.Count > 0)
            {
                Vector2 spawnLoc = Map.Map.map.GetNode<Node2D>("meta/transitions/" + playerState["_mapName"]).GlobalPosition;
                playerState[nameof(Player.GlobalPosition)] = new Godot.Collections.Array(){spawnLoc[0], spawnLoc[1]};
                playerState.Remove("_mapName");
                Player.player.Deserialize(playerState);
            }
        }
        public override void _Process(float delta)
        {
            switch (mapLoader.Poll())
            {
                case Error.Ok:
                    progressBar.Value = 100.0f * mapLoader.GetStage() / mapLoader.GetStageCount();
                    break;
                case Error.FileEof:
                    PackedScene packedScene = (PackedScene) mapLoader.GetResource();
                    cache.Add(mapLoader.GetResource().ResourcePath, packedScene);
                    Node scene = packedScene.Instance();
                    rootNode.AddChild(scene);
                    SetTransitions();
                    SetProcess(false);
                    QueueFree();
                    break;
            }
        }
        public void SetScene(string scenePath, CanvasItem currentScene, bool transition=false)
        {
            rootNode.AddChild(this);

            if (transition && currentScene is Map.Map)
            {
                playerState = Player.player.Serialize();
                playerState.Add("_mapName", Map.Map.map.Name);
            }

            // load map specific data
            Directory directory = new Directory();
            string mapName = scenePath.GetFile().BaseName();
            string dataPath = $"res://data/{mapName}.json";
            string contentPath = $"res://data/{mapName}_content.json";
            string questPath = $"res://data/{mapName}_quest.json";
            if (directory.FileExists(dataPath))
            {
                UnitDB.LoadUnitData(dataPath);
            }
            if (directory.FileExists(contentPath))
            {
                ContentDB.LoadContentData(contentPath);
            }
            if (directory.FileExists(questPath))
            {
                // TODO
                // QuestDB.LoadQuestData(questPath);
            }
            currentScene.Hide();
            CallDeferred(nameof(DeferredSetScene), scenePath, currentScene);
        }
        private void DeferredSetScene(string scenePath, CanvasItem currentScene)
        {
            currentScene.Free();
            if (cache.ContainsKey(scenePath))
            {
                GetTree().ChangeSceneTo(cache[scenePath]);
                CallDeferred(nameof(SetTransitions));
                QueueFree();
            }
            else
            {
                mapLoader = ResourceLoader.LoadInteractive(scenePath);
            }
            SetProcess(true);
        }
    }
}