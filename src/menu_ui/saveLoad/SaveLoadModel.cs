using System.Collections.Generic;
using Game.Database;
using Godot;
namespace Game.Ui
{
	public class SaveLoadModel : Node
	{
		public const string DEFAULT_SAVE_FILE_NAME = "Slot {0}";
		public const int MAX_SAVE_FILES = 8;
		private static Dictionary<int, string> saveFileNames = new Dictionary<int, string>();
		private static readonly File file = new File();

		static SaveLoadModel()
		{
			for (int i = 1; i <= MAX_SAVE_FILES; i++)
			{
				saveFileNames.Add(i, string.Format(DEFAULT_SAVE_FILE_NAME, i));
			}
			// TODO
			// LoadSavedFileNames();
		}
		public static void LoadSavedFileNames()
		{
			Directory directory = new Directory();
			string savePathDir = PathManager.savePath.GetBaseDir();

			// open dir and read files
			directory.Open(savePathDir);
			directory.ListDirBegin(true, true);
			string fileName = directory.GetNext();

			while (!fileName.Empty())
			{
				if (fileName.Extension().Equals(PathManager.dataExt))
				{
					file.Open(savePathDir.PlusFile(fileName), File.ModeFlags.Read);
					saveFileNames[fileName.BaseName().ToInt()] =
						(string)((Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result)[NameDB.SaveTag.TIME];
					file.Close();
				}
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
				{NameDB.SaveTag.MAP, Map.Map.map.Filename},
				{NameDB.SaveTag.TIME, saveTime}
			};

			ISerializable saveNode;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.SAVE_GROUP))
			{
				saveNode = node as ISerializable;
				if (saveNode != null)
				{
					masterSave[node.GetPath()] = saveNode.Serialize();
				}
			}

			// save to file
			file.Open(string.Format(PathManager.savePath, index), File.ModeFlags.Write);
			file.StoreString(JSON.Print(masterSave, "\t", true));
			file.Close();

			saveFileNames[index] = saveTime;
		}
		public void LoadGame(int index)
		{
			string loadPath = string.Format(PathManager.savePath, index);
			if (!file.FileExists(loadPath))
			{
				return;
			}

			// load file
			file.Open(loadPath, File.ModeFlags.Read);
			Godot.Collections.Dictionary payload = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
			file.Close();

			// set scene
			Node root = GetTree().Root;
			CanvasItem recentScene = root.GetChild(root.GetChildren().Count - 1) as CanvasItem;
			if (recentScene != null)
			{
				SceneLoaderController.Init().SetScene((string)payload[NameDB.SaveTag.MAP], recentScene);
			}

			// load data
			foreach (string nodePath in payload.Keys)
			{
				if (HasNode(nodePath))
				{
					(GetNode(nodePath) as ISerializable)?.Deserialize((Godot.Collections.Dictionary)payload[nodePath]);
				}
			}
		}
		public static void DeleteSaveFile(int index)
		{
			Directory directory = new Directory();
			string deletePath = string.Format(PathManager.savePath, index);
			if (directory.FileExists(deletePath))
			{
				directory.Remove(deletePath);
				saveFileNames[index] = string.Format(DEFAULT_SAVE_FILE_NAME, index);
			}
		}
		public static string GetSaveFileName(int index) { return saveFileNames[index]; }
	}
}