using Game.Database;
using System.Collections.Generic;
namespace Game.Quest
{
	public class WorldQuest : ISerializable
	{
		public readonly QuestDB.QuestData quest;
		private readonly Dictionary<string, int> objectives = new Dictionary<string, int>();

		public WorldQuest(QuestDB.QuestData quest)
		{
			this.quest = quest;
			foreach (string objectiveName in quest.objectives.Keys)
			{
				objectives[objectiveName] = quest.objectives[objectiveName].amount;
			}
		}
		public void UpdateQuest(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			if (!objectives.ContainsKey(objectiveName)
			|| quest.objectives[objectiveName].questType != questType
			|| (!countTowardsObjective && quest.objectives[objectiveName].amount == objectives[objectiveName]))
			{
				return;
			}
			objectives[objectiveName] += countTowardsObjective ? -1 : 1;
		}
		public bool IsCompleted()
		{
			foreach (int objectiveAmount in objectives.Values)
			{
				if (objectiveAmount > 0)
				{
					return false;
				}
			}
			return true;
		}
		public void Deserialize(Godot.Collections.Dictionary payload)
		{
			foreach (string objectiveName in payload.Keys)
			{
				objectives[objectiveName] = (int)payload[objectiveName];
			}
		}
		public Godot.Collections.Dictionary Serialize()
		{
			Godot.Collections.Dictionary payload = new Godot.Collections.Dictionary();
			foreach (string objectiveName in objectives.Keys)
			{
				payload.Add(objectiveName, objectives[objectiveName]);
			}
			return payload;
		}
	}
}