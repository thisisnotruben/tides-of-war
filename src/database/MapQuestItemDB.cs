using GC = Godot.Collections;
namespace Game.Database
{
	public class MapQuestItemDB : AbstractDB<MapQuestItemDB.MapQuestItem>
	{
		public class MapQuestItem
		{
			public readonly QuestItem[] mapItems;
			public MapQuestItem(QuestItem[] mapItems) { this.mapItems = mapItems; }
		}
		public class QuestItem
		{
			public readonly string name, type, value;
			public QuestItem(string name, string type, string value)
			{
				this.name = name;
				this.type = type;
				this.value = value;
			}
		}

		public override void LoadData(string path)
		{
			data.Clear();

			GC.Dictionary rawDict = LoadJson(path);
			GC.Array rawMapItems;
			QuestItem[] mapItems;
			int i;

			foreach (string questId in rawDict.Keys)
			{
				rawMapItems = (GC.Array)rawDict[questId];
				mapItems = new QuestItem[rawMapItems.Count];

				i = 0;
				foreach (GC.Dictionary item in rawMapItems)
				{
					mapItems[i++] = new QuestItem(
						name: item[nameof(QuestItem.name)].ToString(),
						type: item[nameof(QuestItem.type)].ToString(),
						value: item[nameof(QuestItem.value)].ToString()
					);
				}

				data.Add(questId, new MapQuestItem(mapItems));
			}
		}
	}
}