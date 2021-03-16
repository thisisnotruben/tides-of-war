using System.Collections.Generic;
using Game.Database;
using Game.Mine;
using Game.Projectile;
using Game.Actor;
using Game.Actor.Doodads;
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
							(((GC.Dictionary)JSON.Parse(file.GetAsText()).Result)[NameDB.SaveTag.TIME]).ToString();
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
			GC.Array landMinesToSave = new GC.Array(),
				missilesToSave = new GC.Array();
			GC.Dictionary masterSave = new GC.Dictionary()
			{
				{NameDB.SaveTag.MAP, Map.Map.map.Filename},
				{NameDB.SaveTag.TIME, saveTime},
				{NameDB.SaveTag.VERSION, 0.0f}
			};

			ISerializable saveNode;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.SAVE_GROUP))
			{
				saveNode = node as ISerializable;
				if (saveNode != null)
				{
					if (node is LandMine)
					{
						landMinesToSave.Add(saveNode.Serialize());
					}
					else if (node is Missile)
					{
						missilesToSave.Add(saveNode.Serialize());
					}
					else if (node is MoveCursorController)
					{
						masterSave[NameDB.SaveTag.CURSOR] = saveNode.Serialize();
					}
					else if (node is Tomb)
					{
						masterSave[NameDB.SaveTag.TOMB] = saveNode.Serialize();
					}
					else if ((node as Npc)?.ShouldSerialize() ?? true)
					{
						masterSave[node.GetPath()] = saveNode.Serialize();
					}
				}
			}
			if (landMinesToSave.Count > 0)
			{
				masterSave[NameDB.SaveTag.LAND_MINES] = landMinesToSave;
			}
			if (missilesToSave.Count > 0)
			{
				masterSave[NameDB.SaveTag.MISSILES] = missilesToSave;
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
			Node root = GetTree().Root,
				recentScene = root.GetChild(root.GetChildren().Count - 1);

			SceneLoaderController.Init().SetScene(payload[NameDB.SaveTag.MAP].ToString(), recentScene, payload);
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