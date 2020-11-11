using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class UnitDB
	{
		public class UnitData
		{
			public readonly string name, img;
			public readonly bool enemy;
			public readonly Vector2 spawnPos;
			public readonly Vector2[] path;
			public readonly int level;

			public UnitData(string name, string img, bool enemy, Vector2 spawnPos, Vector2[] path, int level)
			{
				this.name = name;
				this.img = img;
				this.enemy = enemy;
				this.spawnPos = spawnPos;
				this.path = path;
				this.level = level;
			}
		}
		private static Dictionary<string, UnitData> unitData = new Dictionary<string, UnitData>();

		public static void LoadUnitData(string dbPath)
		{
			// clear out cached database for switching between maps
			unitData.Clear();
			// load & parse data
			File file = new File();
			file.Open(dbPath, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			Godot.Collections.Array spawnPos, rawPath;
			int i;
			Vector2[] path;
			foreach (string itemName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[itemName];

				spawnPos = (Godot.Collections.Array)dict[nameof(UnitData.spawnPos)];

				// get path data
				rawPath = (Godot.Collections.Array)(dict[nameof(UnitData.path)]);
				path = new Vector2[rawPath.Count];
				i = 0;
				foreach (Godot.Collections.Array vectorNode in rawPath)
				{
					path[i++] = new Vector2((float)vectorNode[0], (float)vectorNode[1]);
				}

				unitData.Add(itemName, new UnitData(
					name: (string)dict[nameof(UnitData.name)],
					img: (string)dict[nameof(UnitData.img)],
					enemy: (bool)dict[nameof(UnitData.enemy)],
					spawnPos: new Vector2((float)spawnPos[0], (float)spawnPos[1]),
					path: path,
					level: (int)(Single)dict[nameof(UnitData.level)]
				));
			}
		}
		public static UnitData GetUnitData(string unitEditorName) { return unitData[unitEditorName]; }
		public static bool HasUnitData(string nameCheck) { return unitData.ContainsKey(nameCheck); }
	}
}