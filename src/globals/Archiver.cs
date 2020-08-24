using System.Collections.Generic;
using Godot;
namespace Game
{
	public class Archiver : Node
	{
		public const string DEFAULT_SAVE_FILE_NAME = "Slot {0}";
		public const string SAVE_PATH = "res://data/save/{0}.json";
		public const int MAX_SAVE_FILES = 8;
		private static Dictionary<int, string> saveFileNames = new Dictionary<int, string>();
		private static readonly File file = new File();

		static Archiver()
		{
			for (int i = 1; i <= MAX_SAVE_FILES; i++)
			{
				saveFileNames.Add(i, string.Format(DEFAULT_SAVE_FILE_NAME, i));
			}
			LoadSavedFileNames();
		}
		public static void LoadSavedFileNames()
		{
			Directory directory = new Directory();
			string savePathDir = SAVE_PATH.GetBaseDir();

			// open dir and read files
			directory.Open(savePathDir);
			directory.ListDirBegin(true, true);
			string fileName = directory.GetNext();

			while (!fileName.Empty())
			{
				file.Open(savePathDir.PlusFile(fileName), File.ModeFlags.Read);
				saveFileNames[fileName.BaseName().ToInt()] = 
					(string) ((Godot.Collections.Dictionary) JSON.Parse(file.GetAsText()).Result)["saveTime"];
				file.Close();
				fileName = directory.GetNext();
			}
		}
		public void SaveGame(int index)
		{
			if (index < 1 || index > MAX_SAVE_FILES)
			{
				return;
			}

			Godot.Collections.Dictionary date = OS.GetDatetime();
			string saveTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}", 
				date["year"], date["month"], date["day"],
				date["hour"], date["minute"], date["second"]);

			// create master save and add all components
			Godot.Collections.Dictionary masterSave = new Godot.Collections.Dictionary()
			{
				{"map", Map.Map.map.Filename},
				{"saveTime", saveTime}
			};
			foreach (Node node in GetTree().GetNodesInGroup(Globals.SAVE_GROUP))
			{
				ISerializable saveNode = node as ISerializable;
				if (saveNode != null)
				{
					masterSave[node.GetPath()] = saveNode.Serialize();
				}
			}

			// save to file
			file.Open(string.Format(SAVE_PATH, index), File.ModeFlags.Write);
			file.StoreString(JSON.Print(masterSave, "\t", true));
			file.Close();

			saveFileNames[index] = saveTime;
		}
		public void LoadGame(int index)
		{
			string loadPath = string.Format(SAVE_PATH, index);
			if (!file.FileExists(loadPath))
			{
				return;
			}

			// load file
			file.Open(loadPath, File.ModeFlags.Read);
			Godot.Collections.Dictionary payload = (Godot.Collections.Dictionary) JSON.Parse(file.GetAsText()).Result;
			file.Close();

			// set scene
			Node root = GetTree().Root;
			CanvasItem recentScene = root.GetChild(root.GetChildren().Count - 1) as CanvasItem;
			if (recentScene != null)
			{
				SceneLoader.Init().SetScene((string) payload["map"], recentScene);
			}

			// load data
			foreach (string nodePath in payload.Keys)
			{
				if (HasNode(nodePath))
				{
					ISerializable serializadNode = GetNode(nodePath) as ISerializable;
					if (serializadNode != null)
					{
						serializadNode.Deserialize((Godot.Collections.Dictionary) payload[nodePath]);    
					}
				}
			}
		}
		public static void DeleteSaveFile(int index)
		{
			Directory directory = new Directory();
			string deletePath = string.Format(SAVE_PATH, index);
			if (directory.FileExists(deletePath))
			{
				directory.Remove(deletePath);
				saveFileNames[index] = string.Format(DEFAULT_SAVE_FILE_NAME, index);
			}
		}
		public static string GetSaveFileName(int index)
		{
			return saveFileNames[index];
		}
	}
}