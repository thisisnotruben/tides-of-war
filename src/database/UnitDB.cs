using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class UnitDB
	{
		public struct UnitNode
		{
			public string name, img;
			public bool enemy;
			public Vector2 spawnPos;
			public Vector2[] path;
			public int level;
		}
		private static Dictionary<string, UnitNode> unitData = new Dictionary<string, UnitNode>();

		public static void LoadUnitData(string dbPath)
		{
			// clear out cached database for switching between maps
			unitData.Clear();
			// load & parse data
			File file = new File();
			file.Open(dbPath, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary itemDict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			Godot.Collections.Array spawnPos, rawPath;
			int i;
			foreach (string itemName in rawDict.Keys)
			{
				itemDict = (Godot.Collections.Dictionary)rawDict[itemName];
				UnitNode unitNode;
				unitNode.name = (string)itemDict[nameof(UnitNode.name)];
				unitNode.img = (string)itemDict[nameof(UnitNode.img)];
				unitNode.enemy = (bool)itemDict[nameof(UnitNode.enemy)];
				unitNode.level = (int)((Single)itemDict[nameof(UnitNode.level)]);

				spawnPos = (Godot.Collections.Array)itemDict[nameof(UnitNode.spawnPos)];
				unitNode.spawnPos = new Vector2((float)((Single)spawnPos[0]), (float)((Single)spawnPos[1]));

				// get path data
				rawPath = (Godot.Collections.Array)(itemDict[nameof(UnitNode.path)]);
				unitNode.path = new Vector2[rawPath.Count];
				i = 0;
				foreach (Godot.Collections.Array vectorNode in rawPath)
				{
					unitNode.path[i++] = (new Vector2((float)((Single)vectorNode[0]), (float)((Single)vectorNode[1])));
				}

				unitData.Add(itemName, unitNode);
			}
		}
		public static UnitNode GetUnitData(string unitEditorName) { return unitData[unitEditorName]; }
		public static bool HasUnitData(string nameCheck) { return unitData.ContainsKey(nameCheck); }
	}
}