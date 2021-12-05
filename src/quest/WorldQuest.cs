using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using Game.Ui;
namespace Game.Quest
{
	public class WorldQuest : Node, ISerializable
	{
		public QuestMaster.QuestStatus status = QuestMaster.QuestStatus.UNAVAILABLE;

		public QuestDB.QuestData quest;
		public Dictionary<string, int> objectives { private set; get; }
		private readonly List<string> charactersTalkedTo = new List<string>();

		public WorldQuest() { objectives = new Dictionary<string, int>(); }
		public virtual WorldQuest Init(QuestDB.QuestData quest)
		{
			this.quest = quest;

			foreach (string objectiveName in quest.objectives.Keys)
			{
				objectives[objectiveName] = 0;
			}
			return this;
		}
		public virtual bool UpdateQuest(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			switch (status)
			{
				case QuestMaster.QuestStatus.UNAVAILABLE:
				case QuestMaster.QuestStatus.AVAILABLE:
				case QuestMaster.QuestStatus.DELIVERED:
					return false;
			}

			if (!IsPartOfObjective(objectiveName, questType) || (!countTowardsObjective && objectives[objectiveName] == 0))
			{
				return false;
			}

			objectives[objectiveName] += countTowardsObjective ? 1 : -1;
			if (IsCompleted())
			{
				status = QuestMaster.QuestStatus.COMPLETED;
				GetNodeOrNull<Npc>(quest.questGiverPath)?.questMarker.ShowMarker(QuestMarker.MarkerType.COMPLETED);
			}
			return true;
		}
		public virtual bool UpdateQuest(string characterPath, string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
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
			foreach (string objectiveName in objectives.Keys)
			{
				if (objectives[objectiveName] < quest.objectives[objectiveName].amount)
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

			int i;
			foreach (string objectiveName in quest.objectives.Keys)
			{
				if (!quest.objectives[objectiveName].keepWorldObject)
				{
					for (i = 0; i < quest.objectives[objectiveName].amount; i++)
					{
						playerInventory.RemoveCommodity(objectiveName);
					}
				}
			}
		}
		public bool HasCharacterPath(string characterPath) { return charactersTalkedTo.Contains(characterPath); }
		public virtual void SetEvents()
		{
			foreach (string editorName in Globals.unitDB.data.Keys)
			{
				if (!quest.dialogue.Empty()
				&& quest.dialogue.Equals(Globals.unitDB.GetData(editorName).eventTrigger))
				{
					Map.Map.map.SpawnUnit(editorName,
					   status == QuestMaster.QuestStatus.ACTIVE
					   || status == QuestMaster.QuestStatus.COMPLETED);
				}
			}
		}
		public virtual GC.Dictionary Serialize()
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
		public virtual void Deserialize(GC.Dictionary payload)
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