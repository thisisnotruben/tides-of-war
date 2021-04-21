using Game.Database;
using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using Game.Ui;
namespace Game.Quest
{
	public class WorldQuest : ISerializable
	{
		public QuestMaster.QuestStatus status = QuestMaster.QuestStatus.UNAVAILABLE;

		public readonly QuestDB.QuestData quest;
		private readonly Dictionary<string, int> objectives = new Dictionary<string, int>();
		private readonly List<string> charactersTalkedTo = new List<string>();

		public WorldQuest(QuestDB.QuestData quest)
		{
			this.quest = quest;
			foreach (string objectiveName in quest.objectives.Keys)
			{
				objectives[objectiveName] = quest.objectives[objectiveName].amount;
			}
		}
		public bool UpdateQuest(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			if (status != QuestMaster.QuestStatus.ACTIVE
			|| !IsPartOfObjective(objectiveName, questType)
			|| (!countTowardsObjective && quest.objectives[objectiveName].amount == objectives[objectiveName]))
			{
				return false;
			}

			objectives[objectiveName] += countTowardsObjective ? -1 : 1;
			if (IsCompleted())
			{
				status = QuestMaster.QuestStatus.COMPLETED;
			}
			return true;
		}
		public bool UpdateQuest(string characterPath, string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			if (!charactersTalkedTo.Contains(characterPath)
			&& UpdateQuest(objectiveName, questType, countTowardsObjective))
			{
				charactersTalkedTo.Add(characterPath);
				return true;
			}
			return false;
		}
		public bool IsPartOfObjective(string name, QuestDB.QuestType questType)
		{
			return objectives.ContainsKey(name) && quest.objectives[name].questType == questType;
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
		public void TurnInItems(InventoryModel playerInventory)
		{
			if (!IsCompleted())
			{
				return;
			}

			foreach (string objectiveName in quest.objectives.Keys)
			{
				if (!quest.objectives[objectiveName].keepWorldObject)
				{
					playerInventory.RemoveCommodity(objectiveName);
				}
			}
		}
		public bool HasCharacterPath(string characterPath) { return charactersTalkedTo.Contains(characterPath); }
		public GC.Dictionary Serialize()
		{
			GC.Array charactersPaths = new GC.Array();
			charactersTalkedTo.ForEach(p => charactersPaths.Add(p.ToString()));

			GC.Dictionary payload = new GC.Dictionary()
			{
				{NameDB.SaveTag.NAME, quest.questName},
				{NameDB.SaveTag.TALKED_TO, charactersPaths}
			};

			foreach (string objectiveName in objectives.Keys)
			{
				payload[objectiveName] = objectives[objectiveName];
			}

			return payload;
		}
		public void Deserialize(GC.Dictionary payload)
		{
			string characterPathKey = NameDB.SaveTag.TALKED_TO;
			foreach (string characterPath in (GC.Array)payload[characterPathKey])
			{
				charactersTalkedTo.Add(characterPath);
			}

			payload.Remove(characterPathKey);
			payload.Remove(NameDB.SaveTag.NAME);

			foreach (string objectiveName in payload.Keys)
			{
				objectives[objectiveName] = payload[objectiveName].ToString().ToInt();
			}
		}
	}
}