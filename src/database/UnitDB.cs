using System;
using Godot;
namespace Game.Database
{
	public class UnitDB : AbstractDB<UnitDB.UnitData>
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

		public static readonly UnitDB Instance = new UnitDB();

		public override void LoadData(string path)
		{
			data.Clear();

			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			Godot.Collections.Array spawnPos, rawPath;
			int i;
			Vector2[] unitPath;

			foreach (string itemName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[itemName];

				spawnPos = (Godot.Collections.Array)dict[nameof(UnitData.spawnPos)];

				rawPath = (Godot.Collections.Array)(dict[nameof(UnitData.path)]);
				unitPath = new Vector2[rawPath.Count];
				i = 0;
				foreach (Godot.Collections.Array vectorNode in rawPath)
				{
					unitPath[i++] = new Vector2((float)vectorNode[0], (float)vectorNode[1]);
				}

				data.Add(itemName, new UnitData(
					name: (string)dict[nameof(UnitData.name)],
					img: (string)dict[nameof(UnitData.img)],
					enemy: (bool)dict[nameof(UnitData.enemy)],
					spawnPos: new Vector2((float)spawnPos[0], (float)spawnPos[1]),
					path: unitPath,
					level: (int)(Single)dict[nameof(UnitData.level)]
				));
			}
		}
	}
}