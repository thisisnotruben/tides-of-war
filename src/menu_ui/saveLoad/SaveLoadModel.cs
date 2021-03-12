using System.Collections.Generic;
using Game.Database;
using Godot;
using GC = Godot.Collections;
namespace Game.Ui
{
	public class SaveLoadModel : GameMenu
	{
		public const int MAX_SAVES = 20;
		public static Dictionary<int, string> saveFileNames = new Dictionary<int, string>();
		public static Dictionary<int, Texture> saveFileIcons = new Dictionary<int, Texture>();
		private static readonly File file = new File();
		private Image currentGameImage;

		static SaveLoadModel() { LoadSavedFileNames(); }
		public static void LoadSavedFileNames()
		{
			Directory directory = new Directory();
			string savePathDir = PathManager.savePathDir;
			if (!directory.DirExists(savePathDir))
			{
				return;
			}

			directory.Open(savePathDir);
			directory.ListDirBegin(true, true);
			string fileName = directory.GetNext();

			while (!fileName.Empty())
			{
				switch (fileName.Extension())
				{
					case PathManager.dataExt:
						file.Open(savePathDir.PlusFile(fileName), File.ModeFlags.Read);
						saveFileNames[fileName.BaseName().ToInt()] =
							(string)((GC.Dictionary)JSON.Parse(file.GetAsText()).Result)[NameDB.SaveTag.TIME];
						file.Close();
						break;
					case PathManager.textureExt:
						Image image = new Image();
						image.Load(savePathDir.PlusFile(fileName));
						ImageTexture imageTexture = new ImageTexture();
						imageTexture.CreateFromImage(image);
						saveFileIcons[fileName.BaseName().ToInt()] = imageTexture;
						break;
				}
				fileName = directory.GetNext();
			}
		}
		public int SaveGame()
		{
			for (int i = 0; i < MAX_SAVES; i++)
			{
				if (!saveFileNames.ContainsKey(i))
				{
					return SaveGame(i);
				}
			}
			return -1;
		}
		public int SaveGame(int index)
		{
			GC.Dictionary date = OS.GetDatetime();
			string saveTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}",
				date["year"], date["month"], date["day"],
				date["hour"], date["minute"], date["second"]);

			// create master save and add all components
			GC.Dictionary masterSave = new GC.Dictionary()
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
			Directory directory = new Directory();
			string savePathDir = PathManager.savePathDir;
			if (!directory.DirExists(savePathDir))
			{
				directory.MakeDirRecursive(savePathDir);
			}

			// save data
			file.Open(GetSaveFilePath(index), File.ModeFlags.Write);
			file.StoreString(JSON.Print(masterSave, "\t", true));
			file.Close();

			// save data icon
			currentGameImage.SavePng(GetSaveFilePath(index, false));

			ImageTexture imageTexture = new ImageTexture();
			imageTexture.CreateFromImage(currentGameImage);

			saveFileNames[index] = saveTime;
			saveFileIcons[index] = imageTexture;

			return index;
		}
		public void LoadGame(int index)
		{
			string loadPath = GetSaveFilePath(index);
			if (!file.FileExists(loadPath))
			{
				return;
			}

			// load file
			file.Open(loadPath, File.ModeFlags.Read);
			GC.Dictionary payload = (GC.Dictionary)JSON.Parse(file.GetAsText()).Result;
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
					(GetNode(nodePath) as ISerializable)?.Deserialize((GC.Dictionary)payload[nodePath]);
				}
			}
		}
		public void DeleteSaveFile(int index)
		{
			Directory directory = new Directory();
			string deletePathData = GetSaveFilePath(index),
				deletePathIcon = GetSaveFilePath(index, false);

			if (directory.FileExists(deletePathData))
			{
				directory.Remove(deletePathData);
				saveFileNames.Remove(index);
			}
			if (directory.FileExists(deletePathIcon))
			{
				directory.Remove(deletePathIcon);
				saveFileIcons.Remove(index);
			}
		}
		public void SetCurrentGameImage()
		{
			currentGameImage = GetViewport().GetTexture().GetData();
			currentGameImage.FlipY();
			currentGameImage = currentGameImage.GetRect(new Rect2(
				0.0f, 96.0f, currentGameImage.GetWidth(), currentGameImage.GetWidth()
			));
		}
		private string GetSaveFilePath(int index, bool data = true)
		{
			return PathManager.savePathDir.PlusFile(index + "." + (data ? PathManager.dataExt : PathManager.textureExt));
		}
	}
}