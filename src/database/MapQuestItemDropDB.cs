using GC = Godot.Collections;
using System.Collections.Generic;
namespace Game.Database
{
	public class MapQuestItemDropDB : AbstractDB<MapQuestItemDropDB.QuestDrop>
	{
		public class QuestDrop
		{
			public readonly Dictionary<string, List<string>> drops;
			public QuestDrop(Dictionary<string, List<string>> drops)
			{
				this.drops = drops;
			}
		}

		public override void LoadData(string path)
		{
			data.Clear();

			GC.Dictionary rawDict = LoadJson(path), temp;
			Dictionary<string, List<string>> questDropDict;

			foreach (string questId in rawDict.Keys)
			{
				temp = (GC.Dictionary)rawDict[questId];
				questDropDict = new Dictionary<string, List<string>>();

				foreach (string dropItem in temp.Keys)
				{
					questDropDict[dropItem] = new List<string>();
					foreach (string editorName in (GC.Array)temp[dropItem])
					{
						questDropDict[dropItem].Add(editorName);
					}
				}

				data.Add(questId, new QuestDrop(questDropDict));
			}
		}
		public List<string> GetUnitDrop(string questId, string editorName)
		{
			List<string> dropItemNames = new List<string>();
			if (data.ContainsKey(questId))
			{
				foreach (string dropItem in data[questId].drops.Keys)
				{
					if (data[questId].drops[dropItem].Contains(editorName))
					{
						dropItemNames.Add(dropItem);
					}
				}
			}

			return dropItemNames;
		}
	}
}
