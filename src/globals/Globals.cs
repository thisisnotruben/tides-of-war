using System.Collections.Generic;
using Game.Quests;
using Game.Utils;
using Godot;
namespace Game
{
	public class Globals : Node
	{
		public static readonly Dictionary<string, int> WEAPON_TYPE = new Dictionary<string, int>
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
		public static Dictionary<string, AudioStream> sndData = new Dictionary<string, AudioStream>();
		public static WorldQuests worldQuests { get; private set; }

		public override void _Ready()
		{
			LoadSnd();
		}
		private void LoadSnd(string path = "res://asset/snd")
		{
			sndData.Clear();
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
					sndData.Add(resourceName.BaseName(), (AudioStream)GD.Load(resourcePath));
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public static void PlaySound(string sndName, Node originator, Speaker sndPlayer)
		{
			if (!sndData.ContainsKey(sndName))
			{
				GD.Print($"{nameof(sndData)} doesn't contain sound: {sndName}");
			}
			else
			{
				if (sndPlayer.GetParent() == null)
				{
					originator.AddChild(sndPlayer);
					sndPlayer.Connect("finished", sndPlayer, nameof(Speaker.Delete));
				}
				if (!sndPlayer.Playing)
				{
					sndPlayer.VolumeDb = -10.0f;
					sndPlayer.Stream = (AudioStream)sndData[sndName];
					sndPlayer.Play();
				}
				else
				{
					PackedScene sndScene = (PackedScene)GD.Load("res://src/utils/audio_stream_player.tscn");
					Speaker audioStreamPlayer = (Speaker)sndScene.Instance();
					originator.AddChild(audioStreamPlayer);
					audioStreamPlayer.Connect("finished", audioStreamPlayer, nameof(Speaker.Delete));
					PlaySound(sndName, originator, audioStreamPlayer);
				}
			}
		}
		public static void PlaySound(string sndName, Node originator, Speaker2D sndPlayer)
		{
			if (!sndData.ContainsKey(sndName))
			{
				GD.Print($"{nameof(sndData)} doesn't contain sound: {sndName}");
			}
			else
			{
				if (sndPlayer.GetParent() == null)
				{
					originator.AddChild(sndPlayer);
					sndPlayer.Connect("finished", sndPlayer, nameof(Speaker2D.Delete));
				}
				if (!sndPlayer.Playing)
				{
					sndPlayer.VolumeDb = -10.0f;
					sndPlayer.Stream = (AudioStream)sndData[sndName];
					sndPlayer.Play();
				}
				else
				{
					PackedScene sndScene = (PackedScene)GD.Load("res://src/utils/audio_stream_player_2D.tscn");
					Speaker2D audioStreamPlayer2D = (Speaker2D)sndScene.Instance();
					originator.AddChild(audioStreamPlayer2D);
					audioStreamPlayer2D.Connect("finished", audioStreamPlayer2D, nameof(Speaker2D.Delete));
					PlaySound(sndName, originator, audioStreamPlayer2D);
				}
			}
		}
	}
}