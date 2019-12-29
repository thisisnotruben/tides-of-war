using System.Collections.Generic;
using Game.Actor;
using Game.Map;
using Game.Quests;
using Game.Utils;
using Godot;
namespace Game
{
    public class Globals : Node
    {
        public static readonly PackedScene combatText = (PackedScene)GD.Load("res://src/misc/other/combat_text.tscn");
        public static readonly Dictionary<string, string> SAVE_PATH = new Dictionary<string, string>
        { { nameof(saveData), "res://data/save_game/save_data.json" },
            { "SAVE_SLOT_0", "res://data/save_game/save_slot_0.json" },
            { "SAVE_SLOT_1", "res://data/save_game/save_slot_1.json" },
            { "SAVE_SLOT_2", "res://data/save_game/save_slot_2.json" },
            { "SAVE_SLOT_3", "res://data/save_game/save_slot_3.json" },
            { "SAVE_SLOT_4", "res://data/save_game/save_slot_4.json" },
            { "SAVE_SLOT_5", "res://data/save_game/save_slot_5.json" },
            { "SAVE_SLOT_6", "res://data/save_game/save_slot_6.json" },
            { "SAVE_SLOT_7", "res://data/save_game/save_slot_7.json" }
        };
        public static readonly Dictionary<string, ushort> WEAPON_TYPE = new Dictionary<string, ushort>
        { { "AXE", 5 },
            { "CLUB", 3 },
            { "DAGGER", 3 },
            { "SWORD", 8 },
            { "BOW", 5 },
            { "ARROW_HIT_ARMOR", 5 },
            { "ARROW_PASS", 6 },
            { "SWING_SMALL", 3 },
            { "SWING_MEDIUM", 3 },
            { "SWING_LARGE", 3 },
            { "SWING_VERY_LARGE", 3 },
            { "BLOCK_METAL_METAL", 5 },
            { "BLOCK_METAL_WOOD", 3 },
            { "BLOCK_WOOD_WOOD", 3 },
        };
        public static readonly Dictionary<string, int> Collision = new Dictionary<string, int>
        { { "WORLD", 1 },
            { "CHARACTERS", 2 },
            { "DEAD_CHARACTERS", 3 },
            { "COMBUSTIBLE", 8 }
        };
        public const string HUD_SHORTCUT_GROUP = "HUD-shortcut";
        public const string SAVE_GROUP = "save";
        public static Player player = null;
        public static Dictionary<string, AudioStream> sndMeta = new Dictionary<string, AudioStream>();
        public static Dictionary<string, string> saveData = new Dictionary<string, string>();
        private static Godot.Collections.Dictionary sceneMeta = null;
        private static readonly File file = new File();
        private static WorldQuests worldQuests = null;
        private static Map.Map map = null;
        public override void _Ready()
        {
            LoadsaveData();
            LoadSnd();
            CallDeferred(nameof(SetUpWorldQuests));
        }
        public static void SetScene(string scenePath, Node root, CanvasItem currentScene)
        {
            PackedScene sceneLoaderScene = (PackedScene)GD.Load("res://src/globals/SceneLoader.tscn");
            SceneLoader sceneLoader = (SceneLoader)sceneLoaderScene.Instance();
            root.AddChild(sceneLoader);
            sceneLoader.LoadScene(scenePath, sceneMeta, currentScene);
        }
        public static void SaveGameData(string gameData, int index)
        {
            file.Open(SAVE_PATH[nameof(saveData)], (int)File.ModeFlags.Write);
            saveData[$"slot_{index}"] = gameData;
            file.StoreLine(JSON.Print(saveData));
            file.Close();
        }
        public static void LoadsaveData()
        {
            file.Open(SAVE_PATH[(nameof(saveData))], (int)File.ModeFlags.Read);
            Godot.Collections.Dictionary gameData = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).GetResult();
            file.Close();
            foreach (string key in gameData.Keys)
            {
                saveData.Add(key, (string)gameData[key]);
            }
        }
        private void LoadSnd(string path = "res://asset/snd")
        {
            sndMeta.Clear();
            string importExt = ".import";
            Directory directory = new Directory();
            directory.Open(path);
            directory.ListDirBegin(true, true);
            string resourceName = directory.GetNext();
            string resourcePath;
            while (!resourceName.Empty())
            {
                resourcePath = path.PlusFile(resourceName);
                if (directory.CurrentIsDir())
                {
                    LoadSnd(resourcePath);
                }
                else if (directory.FileExists(resourcePath) && !resourcePath.Contains(importExt))
                {
                    sndMeta.Add(resourceName.BaseName(), (AudioStream)GD.Load(resourcePath));
                }
                resourceName = directory.GetNext();
            }
            directory.ListDirEnd();
        }
        public static void SaveGame(string savePath)
        {
            GameSaver gameSaver = new GameSaver();
            gameSaver.SaveGame(savePath);
        }
        public static void LoadGame(string loadPath)
        {
            if (file.FileExists(loadPath))
            {
                file.Open(loadPath, (int)File.ModeFlags.Read);
                sceneMeta = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).GetResult();
                file.Close();
                SetScene((string)sceneMeta["scene"], GetMap().GetTree().GetRoot(), Globals.GetMap());
            }
        }
        public static void SetMap(Map.Map newMap)
        {
            map = newMap;
        }
        public static Map.Map GetMap()
        {
            return map;
        }
        public static Quests.WorldQuests GetWorldQuests()
        {
            return worldQuests;
        }
        private void SetUpWorldQuests()
        {
            PackedScene worldQuestsScene = (PackedScene)GD.Load("res://src/quest_system/WorldQuests.tscn");
            WorldQuests worldQuests = (WorldQuests)worldQuestsScene.Instance();
            GetTree().GetRoot().AddChild(worldQuests);
            Globals.worldQuests = worldQuests;
        }
        public static void PlaySound(string sndName, Node originator, Speaker sndPlayer)
        {
            if (!sndMeta.ContainsKey(sndName))
            {
                GD.Print($"{nameof(sndMeta)} doesn't contain sound: {sndName}");
            }
            else
            {
                if (sndPlayer.GetParent() == null)
                {
                    originator.AddChild(sndPlayer);
                    sndPlayer.Connect("finished", sndPlayer, nameof(Speaker.Delete));
                }
                if (!sndPlayer.IsPlaying())
                {
                    sndPlayer.SetVolumeDb(-10.0f);
                    sndPlayer.SetStream((AudioStream)sndMeta[sndName]);
                    sndPlayer.Play();
                }
                else
                {
                    PackedScene sndScene = (PackedScene)GD.Load("res://src/utils/AudioStreamPlayer.tscn");
                    Speaker audioStreamPlayer = (Speaker)sndScene.Instance();
                    originator.AddChild(audioStreamPlayer);
                    audioStreamPlayer.Connect("finished", audioStreamPlayer, nameof(Speaker.Delete));
                    PlaySound(sndName, originator, audioStreamPlayer);
                }
            }
        }
        public static void PlaySound(string sndName, Node originator, Speaker2D sndPlayer)
        {
            if (!sndMeta.ContainsKey(sndName))
            {
                GD.Print($"{nameof(sndMeta)} doesn't contain sound: {sndName}");
            }
            else
            {
                if (sndPlayer.GetParent() == null)
                {
                    originator.AddChild(sndPlayer);
                    sndPlayer.Connect("finished", sndPlayer, nameof(Speaker2D.Delete));
                }
                if (!sndPlayer.IsPlaying())
                {
                    sndPlayer.SetVolumeDb(-10.0f);
                    sndPlayer.SetStream((AudioStream)sndMeta[sndName]);
                    sndPlayer.Play();
                }
                else
                {
                    PackedScene sndScene = (PackedScene)GD.Load("res://src/utils/AudioStreamPlayer2D.tscn");
                    Speaker2D audioStreamPlayer2D = (Speaker2D)sndScene.Instance();
                    originator.AddChild(audioStreamPlayer2D);
                    audioStreamPlayer2D.Connect("finished", audioStreamPlayer2D, nameof(Speaker2D.Delete));
                    PlaySound(sndName, originator, audioStreamPlayer2D);
                }
            }
        }
    }
}