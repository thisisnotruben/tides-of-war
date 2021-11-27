using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
namespace Game.Database
{
	public class UnitDB : AbstractDB<UnitDB.UnitData>
	{
		public class UnitData
		{
			public readonly string name, img, dialogue, eventTrigger;
			public readonly bool enemy;
			public readonly int level;
			public readonly float dropRate;
			public readonly Vector2[] path;
			public readonly Vector2 spawnPos;

			public UnitData(string name, string img, bool enemy,
			int level, float dropRate, string dialogue, string eventTrigger,
			Vector2[] path, Vector2 spawnPos)
			{
				this.name = name;
				this.img = img;
				this.enemy = enemy;
				this.level = level;
				this.dropRate = dropRate;
				this.dialogue = dialogue;
				this.eventTrigger = eventTrigger;
				this.path = path;
				this.spawnPos = spawnPos;
			}
		}

		private readonly Dictionary<string, Dictionary<string, UnitData>> cache =
			new Dictionary<string, Dictionary<string, UnitData>>();
		private readonly Dictionary<string, UnitData> tempUnits = new Dictionary<string, UnitData>();

		public UnitDB()
		{
			Directory directory = new Directory();
			directory.Open(PathManager.dataDir);
			directory.ListDirBegin(true, true);

			string resourceName = directory.GetNext(),
				unitDataPath;

			while (!resourceName.Empty())
			{
				if (directory.CurrentIsDir())
				{
					unitDataPath = string.Format(PathManager.unitDataTemplate, resourceName);
					if (directory.FileExists(unitDataPath))
					{
						LoadData(unitDataPath);
					}
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		public override void LoadData(string path)
		{
			string zoneName = path.GetFile().BaseName();
			if (cache.ContainsKey(zoneName))
			{
				this.data = cache[zoneName];
				return;
			}

			GC.Dictionary dict, rawDict = LoadJson(path);
			GC.Array spawnPos, rawPath;
			int i;
			Vector2[] unitPath;

			Dictionary<string, UnitData> data = new Dictionary<string, UnitData>();

			foreach (string itemName in rawDict.Keys)
			{
				dict = (GC.Dictionary)rawDict[itemName];

				spawnPos = (GC.Array)dict[nameof(UnitData.spawnPos)];

				rawPath = (GC.Array)(dict[nameof(UnitData.path)]);
				unitPath = new Vector2[rawPath.Count];
				i = 0;
				foreach (GC.Array vectorNode in rawPath)
				{
					unitPath[i++] = new Vector2((float)vectorNode[0], (float)vectorNode[1]);
				}

				data.Add(itemName, new UnitData(
					name: dict[nameof(UnitData.name)].ToString(),
					img: dict[nameof(UnitData.img)].ToString(),
					enemy: (bool)dict[nameof(UnitData.enemy)],
					level: dict[nameof(UnitData.level)].ToString().ToInt(),
					dropRate: dict[nameof(UnitData.dropRate)].ToString().ToFloat(),
					dialogue: dict[nameof(UnitData.dialogue)].ToString(),
					eventTrigger: dict["event"].ToString(),
					path: unitPath,
					spawnPos: new Vector2((float)spawnPos[0], (float)spawnPos[1])
				));
			}

			cache.Add(zoneName, data);
			this.data = data;
		}
		public override UnitData GetData(string dataName) { return base.GetData(dataName) ?? tempUnits[dataName]; }
		public override bool HasData(string dataName) { return base.HasData(dataName) || tempUnits.ContainsKey(dataName); }
		public UnitData GetDataFromCache(string zoneName, string dataName)
		{
			return cache[zoneName][dataName];
		}
		public bool HasDataFromCache(string zoneName, string dataName)
		{
			return cache.ContainsKey(zoneName) && cache[zoneName].ContainsKey(dataName);
		}
		public void AddTempUnit(string editorName, UnitData unitData) { tempUnits[editorName] = unitData; }
		public bool RemoveTempUnit(string editorName) { return tempUnits.Remove(editorName); }
	}
}