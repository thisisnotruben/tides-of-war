using Godot;

namespace Game
{
    public class SceneLoader : Node
    {
        private ResourceInteractiveLoader mapLoader;
        private Godot.Collections.Dictionary sceneMeta;

        public override void _Ready()
        {
            SetProcess(false);
        }
        public override void _Process(float delta)
        {
            switch (mapLoader.Poll())
            {
                case Error.Ok:
                    GetNode<TextureProgress>("progress_bar/m/v/bar").SetValue(100.0f * mapLoader.GetStage() / mapLoader.GetStageCount());
                    break;
                case Error.FileEof:
                    PackedScene packedScene = ((PackedScene)mapLoader.GetResource());
                    Node scene = packedScene.Instance();
                    GetTree().GetRoot().AddChild(scene);
                    if (scene is Map.Map)
                    {
                        Globals.SetMap(scene as Map.Map);
                    }
                    if (sceneMeta != null && sceneMeta.Count > 0)
                    {
                        foreach (string nodePath in sceneMeta.Keys)
                        {
                            if (!nodePath.Equals("scene"))
                            {
                                GetNode<ISaveable>(nodePath).SetSaveData((Godot.Collections.Dictionary)sceneMeta[nodePath]);
                            }
                        }
                    }
                    Globals.GetWorldQuests().Update();
                    SetProcess(false);
                    QueueFree();
                    break;
            }
        }
        public void LoadScene(string scenePath, Godot.Collections.Dictionary sceneMeta, CanvasItem originator)
        {
            this.sceneMeta = sceneMeta;
            originator.Hide();
            CallDeferred(nameof(DeferredSetScene), scenePath, originator);
        }
        private void DeferredSetScene(string scenePath, CanvasItem originator)
        {
            originator.Free();
            mapLoader = ResourceLoader.LoadInteractive(scenePath);
            SetProcess(true);
        }
    }
}